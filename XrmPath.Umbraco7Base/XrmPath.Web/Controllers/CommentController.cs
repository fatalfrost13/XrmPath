using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using XrmPath.Web.Helpers.UmbracoHelpers;

namespace XrmPath.Web.Controllers
{

    using umbraco.NodeFactory;
    using XrmPath.Helpers.Utilities;
    using XrmPath.Web.Helpers;
    using XrmPath.Web.Models;

    public class CommentController : ApiController
    {

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        // gets all comments for a specific List item id
        public List<CommentItem> GetComments(int id)
        {
            var itemNode = new Node(id);
            var commentList = new List<CommentItem>();
            var commentContainerId = CommentHelper.ContainerId(itemNode.Id);
            var containerNode = new Node(commentContainerId);

            var nodeComments = containerNode.ChildrenAsList.Where(i => i.Parent.Id == containerNode.Id)
                .Select(i => new {NodeId=i.Id, userName = i.GetProperty(CommentHelper.FieldUserName).Value, comment = i.GetProperty(CommentHelper.FieldComment).Value, i.CreateDate });
            commentList.AddRange(nodeComments.Select(i => new CommentItem
                {
                    NodeId=i.NodeId, 
                    UserName=i.userName, 
                    Comment=i.comment, 
                    CreateDate=i.CreateDate, 
                    DisplayCreateDate=i.CreateDate.ToString(CommentHelper.DateFormat)
                }));
            commentList = commentList.OrderByDescending(i => i.CreateDate).ToList();
            return commentList;
        }

        // POST api/<controller>
        public HttpResponseMessage PostComment(CommentItem commentForm)
        {
            var message = CommentFormSubmit(commentForm);
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

        #region Form Submit
        static string CommentFormSubmit(CommentItem commentInfo)
        {
            var containerId = commentInfo.NodeId.ContainerId();
            var itemNode = new Node(commentInfo.NodeId);
            
            var commentNodeName = commentInfo.UserName.Trim() + " - " + DateTime.Now.ToString(CommentHelper.DateFormat);
            var commentDoc = ServiceUtility.ContentService.CreateContent(commentNodeName, containerId, CommentHelper.DoctypeComments);
            commentDoc.SetValue(CommentHelper.FieldUserName, commentInfo.UserName.Trim());
            commentDoc.SetValue(CommentHelper.FieldComment, commentInfo.Comment.Trim().RemoveHtml());


            var requiresApproval = false;
            var requiresApprovalProp = itemNode.GetProperty(CommentHelper.FieldRequiresApproval);
            if (requiresApprovalProp != null)
            {
                requiresApproval = itemNode.GetProperty(CommentHelper.FieldRequiresApproval).Value.FormatBool();
            }
            if (requiresApproval == false)
            {
                ServiceUtility.ContentService.SaveAndPublishWithStatus(commentDoc);
            }
            else
            {
                //requires approval
                ServiceUtility.ContentService.Save(commentDoc);
            }

            string message = requiresApproval == false ? "<font color=green>Your comment has been posted!</font><br /><br />" :
                "<font color=green>Your comment will be posted upon approval.</font><br /><br />";


            return message;
        }


        #endregion
    }
}