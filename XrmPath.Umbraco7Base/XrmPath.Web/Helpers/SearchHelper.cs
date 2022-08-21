using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.SearchCriteria;
using umbraco.NodeFactory;
using XrmPath.Web.Models;
using XrmPath.Helpers.Utilities;
using XrmPath.Web.Helpers.UmbracoHelpers;
using Umbraco.Core.Models;

namespace XrmPath.Web.Helpers
{
    public static class SearchHelper
    {

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

            var criteria = ExamineManager.Instance
                .SearchProviderCollection["ExternalSearcher"]
                .CreateSearchCriteria(BooleanOperation.Or);

            var filter = criteria
                .GroupedOr(fieldParams, searchterm)
                .Compile();

            var searchResults = ExamineManager.Instance.SearchProviderCollection["ExternalSearcher"].Search(filter).ToList();
            var resultItems = GetResultItems(searchResults);

            //for pdf's
            var pdfCriteria = ExamineManager.Instance
                .SearchProviderCollection["PDFSearcher"]
                .CreateSearchCriteria(BooleanOperation.Or);
            var pdfFilter = pdfCriteria
                .GroupedOr(fieldParams, searchterm)
                .Compile();
            var pdfSearchResults = ExamineManager.Instance.SearchProviderCollection["PDFSearcher"].Search(pdfFilter).ToList();
            var pdfResultItems = GetMediaResultItems(pdfSearchResults);
            if (pdfResultItems.Any()) 
            {
                resultItems.AddRange(pdfResultItems);
            }

            resultItems = resultItems.OrderByDescending(i => i.Score).ToList();

            //return ResultItems;
            var searchResultCollection = GetSearchPagination(resultItems, pagesize, currentpage);
            return searchResultCollection;
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

        #region protected methods

        static readonly string searchFields = "title,bodyText,body,description,grid,tags,metaDescription,metaKeywords,FileTextContent";
        public static List<string> SearchFields
        {
            get
            {
                return searchFields.Split(',').ToList();
            }
        }

        static string searchableDocTypes = "homePage,contentPage,article,articleList,link";
        public static List<string> SearchableDocTypes
        {
            get
            {
                return searchableDocTypes.Split(',').ToList();
            }
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

        static List<SearchResultItem> GetResultItems(IEnumerable<SearchResult> searchResults)
        {
            var resultItems = new List<SearchResultItem>();
            var umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();
            foreach (var searchResult in searchResults)
            {
                //only pull certain records
                var searchNode = umbracoHelper.GetById(searchResult.Id);
                if (SearchableDocTypes.Contains(searchResult.Fields["nodeTypeAlias"]) &&
                    resultItems.All(i => i.Id != searchResult.Id))
                {
                    var score = Convert.ToDecimal(searchResult.Score);

                    var resultItem = new SearchResultItem
                    {
                        Id = searchResult.Id,
                        Score = searchNode.WeightedScore(score),
                        OriginalScore = score,
                        Title = searchNode.GetTitle(),
                        //BodyText = SearchResult.Fields[DocumentFields.bodyText.ToString()],
                        Type = "content",
                        ShortDescription = GetSearchResultItem(searchResult, "description,metaDescription"),
                        Url = SearchResultUrl(searchResult)
                    };

                    if (string.IsNullOrEmpty(resultItem.ShortDescription))
                    {
                        resultItem.ShortDescription = GetSearchResultItem(searchResult, "bodyText").TruncateText(" ...", 200).RemoveHtml();
                    }

                    resultItems.Add(resultItem);
                }
                else
                {
                    //check if node is a sub node of an article then proceed according to business rules.
                    var topLevelNodeId = FindTopLevelNodeId(searchResult.Id);
                    if (topLevelNodeId > 0)
                    {
                        var topLevelNode = umbracoHelper.GetById(topLevelNodeId);
                        var resultItemCount = resultItems.Count(i => i.Id == topLevelNodeId);
                        var score = Convert.ToDecimal(searchResult.Score);

                        switch (resultItemCount)
                        {
                            case 0:
                                {
                                    //add top level item to search
                                    
                                    var resultItem = new SearchResultItem
                                    {
                                        Id = topLevelNode.Id,
                                        Score = topLevelNode.WeightedScore(score),
                                        OriginalScore = score,
                                        Title = topLevelNode.GetTitle(),
                                        //BodyText = topLevelNode.GetNodeValue(DocumentFields.bodyText.ToString()),
                                        Url = topLevelNode.GetUrl(),
                                        Type = "content",
                                        ShortDescription = topLevelNode.GetContentValue("description")
                                    };
                                    resultItems.Add(resultItem);
                                }
                                break;
                            case 1:
                                {
                                    //already exists, need to update the score of the ResultItem Record accordingly
                                    //SearchResultItem NewResultItem = ResultItems.Where(i => i.Id == topLevelNodeId).SingleOrDefault();
                                    //SearchResultItem OldResultItem = NewResultItem;
                                    //NewResultItem.Score += SearchResult.Score;
                                    //ResultItems.Remove(OldResultItem);
                                    //ResultItems.Add(NewResultItem);

                                    var index = resultItems.FindIndex(i => i.Id == topLevelNodeId);
                                    var newResultItem = resultItems[index];
                                    newResultItem.Score += topLevelNode.WeightedScore(score);
                                    newResultItem.OriginalScore = score;
                                    resultItems[index] = newResultItem;
                                }
                                break;
                        }
                    }
                }
            }
            return resultItems;
        }

        static List<SearchResultItem> GetMediaResultItems(IEnumerable<SearchResult> searchResults)
        {
            var resultItems = new List<SearchResultItem>();
            var umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();
            foreach (var searchResult in searchResults)
            {
                //only pull certain records
                //var searchNode = umbracoHelper.GetById(searchResult.Id);
                var mediaNode = MediaUtility.GetMediaItem(searchResult.Id);
                var mediaUrl = mediaNode.Url;

                var resultItem = new SearchResultItem
                {
                    Id = searchResult.Id,
                    Score = Convert.ToDecimal(searchResult.Score),
                    Title = mediaNode.Name,
                    //BodyText = SearchResult.Fields[DocumentFields.bodyText.ToString()],
                    ShortDescription = GetSearchResultItem(searchResult, "FileTextContent").TruncateText("...", 200),
                    Type = "media",
                    Url = mediaUrl
                };

                if (string.IsNullOrEmpty(resultItem.ShortDescription))
                {
                    resultItem.ShortDescription = GetSearchResultItem(searchResult, "bodyText").TruncateText(" ...", 200).RemoveHtml();
                }

                resultItems.Add(resultItem);
                
            }
            return resultItems;
        }

        static string SearchResultUrl(SearchResult searchResult)
        {
            var umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();
            var searchUrl = "";
            var nodeId = searchResult.Id;
            var node = umbracoHelper.GetById(nodeId);
            if (node.Id > 0)
            {
                searchUrl = node.GetUrl();
            }

            return searchUrl;
        }

        static string GetSearchResultItem(SearchResult searchResult, string fields)
        {
            var searchItem = "";
            var fieldList = fields.Split(',');

            foreach (var field in fieldList) 
            {
                if (string.IsNullOrEmpty(searchItem))
                {
                    if (searchResult.Fields.ContainsKey(field))
                    {
                        searchItem = searchResult.Fields[field];
                    }
                }
            }
            
            return searchItem;
        }


        static IEnumerable<string> GetFields()
        {
            //var paramFields = SearchFields.Replace(", ", ",");
            //var pageFields = paramFields.Split(',');
            var pageFields = SearchFields.Select(i => i);
            return pageFields;
        }

        static int FindTopLevelNodeId(int id)
        {
            var exitLoop = false;
            var topNodeId = 0;
            var topNode = ServiceUtility.UmbracoHelper.GetById(id);
            while (exitLoop == false)
            {

                if (SearchableDocTypes.Contains(topNode.DocumentTypeAlias))
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
        #endregion
    }
}