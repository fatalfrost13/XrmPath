using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using XrmPath.UmbracoCore.Models;
using XrmPath.Helpers.Utilities;
using Umbraco.Core.Models;
using XrmPath.UmbracoCore.Utilities;
using Umbraco.Core.Models.PublishedContent;
using XrmPath.UmbracoCore.Models.PaginationModels;
using Examine.Search;
using XrmPath.UmbracoCore.SearchModels;
using XrmPath.Helpers.Model;
using XrmPath.UmbracoCore.Models.SearchModels;

namespace XrmPath.UmbracoCore.Helpers
{
    public static class SearchHelper
    {

        /// <summary>
        /// Documentation can be found here:
        ///https://our.umbraco.com/documentation/Reference/Searching/Examine/quick-start/
        /// </summary>

        private static readonly SearchResultItemPager searchCollection = new SearchResultItemPager { SearchResultItems = null };


        // GET api/<controller>
        public static SearchResultItemPager GetEmptySearchResultCollection()
        {
            //return GetSearchResultCollection("test", 10, 1);
            return searchCollection;
        }

        // GET api/<controller>/5
        // ReSharper disable CSharpWarnings::CS1570
        ///example of how this webservice is called.
        ///api/search/?searchterm=test&pagesize=10&currentpage=1
        // ReSharper restore CSharpWarnings::CS1570
        public static SearchResultItemPager GetSearchResultPager(string searchterm, int pagesize, int currentpage)
        {
            var fieldParams = GetFields();

            //var searchTerm = searchterm;
            //if (searchTerm == null)
            //{

            //}

            var resultItems = new List<SearchResultItem>();
            ISearchResults results;


            results = QuerySearchIndex(searchterm, "ExternalIndex");
            
            if (results.Any())
            {
                //var test = (SearchResult)results;
                var resultsList = results.ToList();
                var contentResults = GetContentResultItems(resultsList);
                resultItems.AddRange(contentResults);
            }

            ////https://github.com/umbraco/UmbracoExamine.PDF
            results = QuerySearchIndex(searchterm, "PDFIndex");
            if (results.Any())
            {
                //var test = (SearchResult)results;
                var resultsList = results.ToList();
                var mediaResults = GetMediaResultItems(resultsList);
                resultItems.AddRange(mediaResults);
            }



            resultItems = resultItems.OrderByDescending(i => i.Score).ToList();

            // //return ResultItems;
            var searchResultCollection = GetSearchPagination(resultItems, pagesize, currentpage);
            return searchResultCollection;
        }

        private static ISearchResults QuerySearchIndex(string searchTerm, string indexName = "ExternalIndex")
        {
            if (!ExamineManager.Instance.TryGetIndex(indexName, out var index))
            {
                throw new InvalidOperationException($"No index found with name {indexName}");
            }
            ISearcher searcher = index.GetSearcher();
            IQuery query = searcher.CreateQuery(null, BooleanOperation.And);
            //string searchFields = "nodeName,pageTitle,metaDescription,bodyText";
            var searchFields = ConfigurationModel.SearchableFields;
            IBooleanOperation terms = query.GroupedOr(searchFields.Split(','), searchTerm);
            return terms.Execute();
        }

        public static decimal WeightedScore(this IPublishedContent content, decimal score, decimal applyMultiplier = 1)
        {
            var weightedScore = score;
            var weight = content.GetNodeDecimal("weight", 1);

            if (weight != 1)
            {
                weightedScore = score * weight;
            }

            //if (!gsaEnabled)
            //{
            //    weightedScore += 1; //set a base of 1
            //}

            weightedScore = weightedScore * applyMultiplier;

            return weightedScore;
        }

        static SearchResultItemPager GetSearchPagination(List<SearchResultItem> resultItems, int pagesize, int currentpage)
        {

            var resultCount = resultItems.Count;
            var paginationValues = PaginationExtensions.PaginationValues(pagesize, currentpage, resultCount);
            var resultItemsSubset = resultItems;
            if (paginationValues.ValidCurrentPage && paginationValues.ValidPageSize)
            {
                resultItemsSubset = resultItems.Skip(paginationValues.IndexStart).Take(paginationValues.PageSize).ToList();
            }
            var searchResultCollection = new SearchResultItemPager
            {
                SearchResultItems = resultItemsSubset,
                Pagination = paginationValues
            };
            return searchResultCollection;
        }

        static List<SearchResultItem> GetContentResultItems(IEnumerable<ISearchResult> searchResults)
        {
            var resultItems = new List<SearchResultItem>();
            foreach (var searchResult in searchResults)
            {
                var contentType = searchResult.GetValues("__IndexType").FirstOrDefault();
                var documentType = searchResult.GetValues("__NodeTypeAlias").FirstOrDefault();
                if (contentType == "content") 
                { 
                    //only pull certain records
                    var searchResultId = int.Parse(searchResult.Id);
                    var searchNode = ServiceUtility.UmbracoHelper.GetById(searchResultId);
                    //if (SearchableDocTypes.Contains(searchResult.Fields["nodeTypeAlias"]) &&
                    if (ConfigurationModel.SearchableContentTypesList.Contains(documentType) && resultItems.All(i => i.Id.ToString() != searchResult.Id))
                    {
                        var score = Convert.ToDecimal(searchResult.Score);
                        var weightedScore = searchNode.WeightedScore(score);

                        SearchResultItem resultItem = new SearchResultItem();

                        var resultItemCount = resultItems.Count(i => i.Id == searchResultId);
                        if (resultItemCount == 0)
                        {
                            resultItem = new SearchResultItem
                            {
                                Id = searchResultId,
                                Score = weightedScore,
                                OriginalScore = score,
                                Title = searchNode.GetTitle(),
                                Type = "content",
                                ShortDescription = GetSearchResultItem(searchResult, $"{UmbracoCustomFields.Description},{UmbracoCustomFields.MetaDescription}"),
                                SubScores = new List<SearchScore> { new SearchScore { NodeId = searchResultId, Score = weightedScore, OriginalScore = score } },
                                Url = SearchResultUrl(searchResult)
                            };
                        }
                        else
                        {
                            //already exists, populate with node data
                            var index = resultItems.FindIndex(i => i.Id == searchResultId);
                            var existingResultItem = resultItems[index];
                            existingResultItem.Score = weightedScore;
                            existingResultItem.OriginalScore = score;
                            existingResultItem.SubScores.Add(new SearchScore { NodeId = searchResultId, Score = weightedScore, OriginalScore = score });
                            resultItems[index] = existingResultItem;
                        }

                        if (string.IsNullOrEmpty(resultItem.ShortDescription))
                        {
                            resultItem.ShortDescription = GetSearchResultItem(searchResult, "bodyText").TruncateText(" ...", 200).RemoveHtml();
                        }

                        resultItems.Add(resultItem);
                    }
                    else
                    {
                        //check if node is a sub node of an article then proceed according to business rules.
                        var topLevelNodeId = FindTopLevelNodeId(searchResultId);
                        if (topLevelNodeId > 0)
                        {
                            var topLevelNode = ServiceUtility.UmbracoHelper.GetById(topLevelNodeId);
                            var resultItemCount = resultItems.Count(i => i.Id == topLevelNodeId);
                            var score = Convert.ToDecimal(searchResult.Score);
                            var weightedScore = topLevelNode.WeightedScore(score);
                            switch (resultItemCount)
                            {
                                case 0:
                                    {
                                        //add top level item to search
                                        var resultItem = new SearchResultItem
                                        {
                                            Id = topLevelNode.Id,
                                            //Score = weightedScore,
                                            //OriginalScore = score,
                                            Title = topLevelNode.GetTitle(),
                                            Url = topLevelNode.GetUrl(),
                                            Type = "content",
                                            ShortDescription = topLevelNode.GetContentValue(UmbracoCustomFields.Description),
                                            SubScores = new List<SearchScore> { new SearchScore { NodeId = searchResultId, Score = weightedScore, OriginalScore = score } }
                                        };
                                        resultItems.Add(resultItem);
                                    }
                                    break;
                                case 1:
                                    {
                                        //already exists, need to update the score of the ResultItem Record accordingly
                                        var index = resultItems.FindIndex(i => i.Id == topLevelNodeId);
                                        var newResultItem = resultItems[index];
                                        //newResultItem.Score += weightedScore;
                                        //newResultItem.OriginalScore = score;
                                        newResultItem.SubScores.Add(new SearchScore { NodeId = searchResultId, Score = weightedScore, OriginalScore = score });
                                        resultItems[index] = newResultItem;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            resultItems = EvaluateSubnodeScores(resultItems);
            return resultItems;
        }

        /// <summary>
        /// This method handles sub nodes of a search result item that are rolled up because they aren't direct website content (ex. comments)
        /// </summary>
        /// <param name="resultItems"></param>
        /// <returns></returns>
        public static List<SearchResultItem> EvaluateSubnodeScores(List<SearchResultItem> resultItems) {

            //new code
            //need to evaluate nodes with multiple subnode scores
            var resultIndex = 0;
            var evaluatedResultList = new List<SearchResultItem>();
            foreach (var resultItem in resultItems)
            {
                var updatedResultItem = resultItem;

                if (resultItem.SubScores.Count > 1)
                {
                    //var editResultItem = resultItems[index];
                    //multiple scores for 1 items
                    decimal originalScore = 0;
                    decimal weightedScore = 0;
                    var sourceNode = resultItem.SubScores.FirstOrDefault(i => i.NodeId == resultItem.Id);
                    if (sourceNode != null)
                    {
                        //source node exists in search results
                        // originalScore = sourceNode.
                        originalScore = sourceNode.OriginalScore;
                        weightedScore = sourceNode.Score;

                    }

                    //get average weight score of sub nodes and add that to the root weighted score
                    var subScoreList = resultItem.SubScores.Where(i => i.NodeId != resultItem.Id).ToList();
                    var avgSubScores = subScoreList.Select(i => i.Score).Average();
                    updatedResultItem.OriginalScore = originalScore;
                    updatedResultItem.Score += avgSubScores;
                    evaluatedResultList.Add(updatedResultItem);
                }
                else 
                {
                    var subScore = resultItem.SubScores.SingleOrDefault();
                    if (subScore != null)
                    {
                        updatedResultItem.OriginalScore = subScore.OriginalScore;
                        updatedResultItem.Score = subScore.Score;
                        evaluatedResultList.Add(updatedResultItem);
                    }
                }
                
                resultIndex++;
            }

            return evaluatedResultList;
        }

        static List<SearchResultItem> GetMediaResultItems(IEnumerable<ISearchResult> searchResults)
        {
            var resultItems = new List<SearchResultItem>();
            
            foreach (var searchResult in searchResults)
            {
                //only pull certain records
                //var searchNode = umbracoHelper.GetById(searchResult.Id);

                var searchResultId = int.Parse(searchResult.Id);
                var mediaNode = MediaUtility.GetMediaItem(searchResultId);
                if (mediaNode != null)
                {
                    var mediaUrl = mediaNode.Url;
                    var resultItem = new SearchResultItem
                    {
                        Id = searchResultId,
                        Score = Convert.ToDecimal(searchResult.Score),  //no weighted score for media
                        OriginalScore = Convert.ToDecimal(searchResult.Score),
                        Title = mediaNode.Name,
                        //BodyText = SearchResult.Fields[DocumentFields.bodyText.ToString()],
                        ShortDescription = GetSearchResultItem(searchResult, UmbracoCustomFields.FileTextContent).TruncateText("...", 200),
                        Type = "media",
                        Url = mediaUrl
                    };

                    if (string.IsNullOrEmpty(resultItem.ShortDescription))
                    {
                        resultItem.ShortDescription = GetSearchResultItem(searchResult, UmbracoCustomFields.Description).TruncateText(" ...", 200).RemoveHtml();
                    }
                    resultItems.Add(resultItem);
                }
            }
            return resultItems;
        }

        static string SearchResultUrl(ISearchResult searchResult)
        {
            var searchUrl = "";
            var nodeId = int.Parse(searchResult.Id);
            var node = ServiceUtility.UmbracoHelper.GetById(nodeId);
            if (node.Id > 0)
            {
                searchUrl = node.GetUrl();
            }

            return searchUrl;
        }

        static string GetSearchResultItem(ISearchResult searchResult, string fields)
        {
            var searchItem = "";
            var fieldList = fields.Split(',');

            foreach (var field in fieldList) 
            {
                if (string.IsNullOrEmpty(searchItem))
                {
                    var searchValue = searchResult.GetValues(field).FirstOrDefault();
                    if (searchValue != null)
                    {
                        searchItem = searchValue;
                    }
                    //if (searchResult.Fields.ContainsKey(field))
                    //{
                    //    searchItem = searchResult.Fields[field];
                    //}
                }
            }
            
            return searchItem;
        }


        static IEnumerable<string> GetFields()
        {
            //var paramFields = SearchFields.Replace(", ", ",");
            //var pageFields = paramFields.Split(',');
            var pageFields = ConfigurationModel.SearchableFieldsList.Select(i => i);
            return pageFields;
        }

        static int FindTopLevelNodeId(int id)
        {
            var exitLoop = false;
            var topNodeId = 0;
            var topNode = ServiceUtility.UmbracoHelper.GetById(id);
            while (exitLoop == false)
            {

                if (ConfigurationModel.SearchableContentTypesList.Contains(topNode.ContentType.Alias))
                {
                    topNodeId = topNode.Id;
                    exitLoop = true;
                }
                else
                {
                    //keep looking up at the parent directory
                    if (topNode.Parent != null && topNode.Parent.Id > 0)
                    {
                        topNode = ServiceUtility.UmbracoHelper.GetById(topNode.Parent.Id);
                    }
                    else
                    {
                        exitLoop = true;
                    }
                    if (topNode.Id == 0)
                    {
                        exitLoop = true;
                    }
                }
            }

            return topNodeId;
        }

    }
}