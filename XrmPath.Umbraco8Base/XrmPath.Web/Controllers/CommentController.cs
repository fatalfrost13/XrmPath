using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using XrmPath.UmbracoCore.Helpers;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Controllers
{


    public class CommentController : ApiController
    {

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        // gets all comments for a specific List item id
        public List<CommentModel> GetComments(int id)
        {
            var commentList = CommentHelper.GetComments(id);
            return commentList;
        }

        // POST api/<controller>
        public HttpResponseMessage PostComment(CommentModel commentForm)
        {
            var message = CommentHelper.CommentFormSubmit(commentForm);
            var response = Request.CreateResponse(HttpStatusCode.Accepted, message);
            response.Content = new StringContent(message);
            return response;
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