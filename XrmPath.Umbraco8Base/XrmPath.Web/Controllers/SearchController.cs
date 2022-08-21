using System.Web.Http;
using XrmPath.UmbracoCore.Models;
using XrmPath.UmbracoCore.Helpers;
using XrmPath.UmbracoCore.SearchModels;

namespace IomerBase.U6.ApiControllers
{
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