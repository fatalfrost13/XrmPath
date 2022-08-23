using Examine;
using XrmPath.Helpers.Utilities;
using Examine.Search;
using XrmPath.UmbracoCore.Models;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using XrmPath.UmbracoCore.Definitions;

namespace XrmPath.UmbracoCore.Utilities
{
    public class SearchUtility
    {

        private readonly PublishedContentUtility _pcUtil;
        private readonly UmbracoHelper? _umbracoHelper;
        private readonly IMediaService? _mediaService;
        private readonly IExamineManager? _examineManager;
        private readonly MediaUtility? _mediaUtil;
        public SearchUtility(PublishedContentUtility pcUtil)
        {
            _pcUtil = pcUtil;

            if (_umbracoHelper == null)
            {
                _umbracoHelper = _pcUtil.GetUmbracoHelper();
            }
            if (_mediaService == null)
            {
                _mediaService = _pcUtil.GetMediaService();
            }
            if (_examineManager == null)
            {
                _examineManager = _pcUtil.GetExamineManager();
            }
            if (_mediaUtil == null)
            {
                _mediaUtil = new MediaUtility(pcUtil);

            }
        }

        /// <summary>
        /// Documentation can be found here:
        ///https://our.umbraco.com/documentation/Reference/Searching/Examine/quick-start/
        /// </summary>

        private readonly SearchResultItemPager searchCollection = new SearchResultItemPager { SearchResultItems = null };


        // GET api/<controller>
        public SearchResultItemPager GetEmptySearchResultCollection()
        {
            //return GetSearchResultCollection("test", 10, 1);
            return searchCollection;
        }

        // GET api/<controller>/5
        // ReSharper disable CSharpWarnings::CS1570
        ///example of how this webservice is called.
        ///api/search/?searchterm=test&pagesize=10&currentpage=1
        // ReSharper restore CSharpWarnings::CS1570
        public SearchResultItemPager GetSearchResultPager(string searchterm, int pagesize, int currentpage)
        {
            var fieldParams = GetFields();

            //var searchTerm = searchterm;
            //if (searchTerm == null)
            //{

            //}

            var resultItems = new List<SearchResultItem>();
            ISearchResults? results;


            results = QuerySearchIndex(searchterm, "ExternalIndex");
            
            if (results != null && results.Any())
            {
                //var test = (SearchResult)results;
                var resultsList = results.ToList();
                var contentResults = GetContentResultItems(resultsList);
                resultItems.AddRange(contentResults);
            }

            ////https://github.com/umbraco/UmbracoExamine.PDF
            results = QuerySearchIndex(searchterm, "PDFIndex");
            if (results != null && results.Any())
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

        private ISearchResults? QuerySearchIndex(string searchTerm, string indexName = "ExternalIndex")
        {
            IIndex? index = null;
            var indexExists = _examineManager?.TryGetIndex(indexName, out index) ?? false;
           
            if (indexExists)
            {
                var searcher = index?.Searcher;
                IQuery? query = searcher?.CreateQuery(null, BooleanOperation.And);
                //string searchFields = "nodeName,pageTitle,metaDescription,bodyText";
                var searchFields = ConfigurationModel.SearchableFields;
                IBooleanOperation? terms = query?.GroupedOr(searchFields.Split(','), searchTerm);
                return terms?.Execute();
            }
            else {
                //throw new InvalidOperationException($"No index found with name {indexName}");
                return null;
            }
        }

        public decimal WeightedScore(IPublishedContent? content, decimal score, decimal applyMultiplier = 1)
        {
            var weightedScore = score;
            var weight = _pcUtil.GetNodeDecimal(content, "weight", 1);

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

        private SearchResultItemPager GetSearchPagination(List<SearchResultItem> resultItems, int pagesize, int currentpage)
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

        private List<SearchResultItem> GetContentResultItems(IEnumerable<ISearchResult> searchResults)
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
                    var searchNode = _umbracoHelper?.Content(searchResultId);
                    //if (SearchableDocTypes.Contains(searchResult.Fields["nodeTypeAlias"]) &&
                    if (documentType != null && ConfigurationModel.SearchableContentTypesList.Contains(documentType) && resultItems.All(i => i.Id.ToString() != searchResult.Id))
                    {
                        var score = Convert.ToDecimal(searchResult.Score);
                        var weightedScore = WeightedScore(searchNode, score);

                        SearchResultItem resultItem = new SearchResultItem();

                        var resultItemCount = resultItems.Count(i => i.Id == searchResultId);
                        if (resultItemCount == 0)
                        {
                            resultItem = new SearchResultItem
                            {
                                Id = searchResultId,
                                Score = weightedScore,
                                OriginalScore = score,
                                Title = _pcUtil.GetTitle(searchNode),
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
                            var topLevelNode = _umbracoHelper?.Content(topLevelNodeId);
                            var resultItemCount = resultItems.Count(i => i.Id == topLevelNodeId);
                            var score = Convert.ToDecimal(searchResult.Score);
                            var weightedScore = WeightedScore(topLevelNode, score);
                            switch (resultItemCount)
                            {
                                case 0:
                                    {
                                        //add top level item to search
                                        var resultItem = new SearchResultItem
                                        {
                                            Id = topLevelNode?.Id ?? 0,
                                            //Score = weightedScore,
                                            //OriginalScore = score,
                                            Title = _pcUtil.GetTitle(topLevelNode),
                                            Url = _pcUtil.GetUrl(topLevelNode),
                                            Type = "content",
                                            ShortDescription = _pcUtil.GetContentValue(topLevelNode, UmbracoCustomFields.Description),
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
        public List<SearchResultItem> EvaluateSubnodeScores(List<SearchResultItem> resultItems) {

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

        private List<SearchResultItem> GetMediaResultItems(IEnumerable<ISearchResult> searchResults)
        {
            var resultItems = new List<SearchResultItem>();
            
            foreach (var searchResult in searchResults)
            {
                //only pull certain records
                //var searchNode = umbracoHelper.GetById(searchResult.Id);

                var searchResultId = int.Parse(searchResult.Id);
                var mediaNode = _mediaUtil?.GetMediaItem(searchResultId);
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

        private string SearchResultUrl(ISearchResult searchResult)
        {
            var searchUrl = "";
            var nodeId = int.Parse(searchResult.Id);
            var node = _umbracoHelper?.Content(nodeId);
            if (node?.Id > 0)
            {
                searchUrl = _pcUtil.GetUrl(node);
            }

            return searchUrl;
        }

        private string GetSearchResultItem(ISearchResult searchResult, string fields)
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


        private IEnumerable<string> GetFields()
        {
            //var paramFields = SearchFields.Replace(", ", ",");
            //var pageFields = paramFields.Split(',');
            var pageFields = ConfigurationModel.SearchableFieldsList.Select(i => i);
            return pageFields;
        }

        private int FindTopLevelNodeId(int id)
        {
            var exitLoop = false;
            var topNodeId = 0;
            var topNode = _umbracoHelper?.Content(id);

            if (topNode == null) {
                return 0;
            }
            while (exitLoop == false)
            {
                if (topNode == null) {
                    exitLoop = true;
                }
                else if (ConfigurationModel.SearchableContentTypesList.Contains(topNode.ContentType.Alias))
                {
                    topNodeId = topNode.Id;
                    exitLoop = true;
                }
                else
                {
                    //keep looking up at the parent directory
                    if (topNode.Parent != null && topNode.Parent.Id > 0)
                    {
                        topNode = _umbracoHelper?.Content(topNode.Parent.Id);
                    }
                    else
                    {
                        exitLoop = true;
                    }
                    if (topNode?.Id == 0)
                    {
                        exitLoop = true;
                    }
                }
            }

            return topNodeId;
        }

    }
}