using System.Linq;
using System.Web.Http;

namespace IomerBase.U6.ApiControllers
{
    using System;
    using System.Collections.Generic;
    using Examine;
    using Examine.SearchCriteria;
    using umbraco.NodeFactory;
    using XrmPath.Web.Models;
    using XrmPath.Helpers.Utilities;
    using XrmPath.Web.Helpers.UmbracoHelpers;
    using XrmPath.Web.Helpers;

    public class SearchController : ApiController
    {
        // GET api/<controller>/
        public SearchResultItemPager GetSearchResults(string searchterm, int pagesize, int currentpage) 
        {
            var results = SearchHelper.GetSearchResultPager(searchterm, pagesize, currentpage);
            return results;
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }

    
    }
}