using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Utilities;
using XrmPath.Web.Definitions;
using XrmPath.Web.Models;

namespace XrmPath.Web.Helpers
{
    /// <summary>
    /// Dependencies: Logger(optional), UmbracoHelper, ContentService
    /// </summary>
    /// <param name="serviceUtil"></param>
    public class CommentHelper: BaseInitializer
    {
        public CommentHelper(ServiceUtility serviceUtil) : base(serviceUtil) { }



        public int ContainerId(int listItemId)
        {
            //int[] containerId = { 0 };
            var containerId = 0;
            if (umbracoHelper == null) {
                return containerId;
            }
            //var listNode = new Node(listItemId);
            var listNode = umbracoHelper.Content(listItemId);

            if (listNode?.Id > 0)
            {
                var container = listNode?.Children?.FirstOrDefault(i => i.ContentType.Alias.Equals(CommentFields.DoctypeContainer));
                //foreach (var container in from IPublishedContent container in listNode.Children where containerId[0] == 0 where container.DocumentTypeAlias.ToLower() == DoctypeContainer select container)
                //{
                //    containerId[0] = container.Id;
                //}

                if (container == null && listNode != null && contentService != null)
                {
                    //no container. need to create a container
                    var containerDoc = contentService.Create(CommentFields.NameCommentsContainer, listNode.Id, CommentFields.DoctypeContainer);
                    //containerDoc.SetValue("title", NameCommentsContainer);
                    if (containerDoc != null) {
                        contentService?.SaveAndPublish(containerDoc);
                        containerId = containerDoc?.Id ?? 0;
                    }
                }
                else
                {
                    containerId = container?.Id ?? 0;
                }
            }
            return containerId;
        }
        public bool ShowComments(int nodeid)
        {
            var showComments = false;
            if (umbracoHelper == null || pcUtil == null) {
                return showComments;
            }
            var node = umbracoHelper.Content(nodeid);
            if (pcUtil.NodeExists(node))
            {
                var allowComments = false;
                var allowCommentsProp = node?.GetProperty(CommentFields.FieldAllowComments);
                if (allowCommentsProp != null)
                {
                    allowComments = pcUtil.GetNodeBoolean(node, CommentFields.FieldAllowComments);
                }

                if (allowComments)
                {
                    //show comments
                    showComments = true;
                }
            }
            return showComments;
        }
        public string ViewComments(int nodeid, string url)
        {
            var strComments = "";
            var count = 0;

            if (umbracoHelper != null && ShowComments(nodeid))
            {
                var containerId = ContainerId(nodeid);
                var containerNode = umbracoHelper.Content(containerId);
                if (containerNode != null)
                {
                    var containerChildren = containerNode.Children ?? Enumerable.Empty<IPublishedContent>();
                    foreach (var commentNode in containerChildren)
                    {
                        var hidden = false;
                        var hiddenProp = commentNode.GetProperty(CommentFields.FieldHidden);
                        if (hiddenProp != null)
                        {
                            hidden = pcUtil?.GetNodeBoolean(commentNode, CommentFields.FieldHidden) ?? false;
                        }

                        if (hidden == false)
                        {
                            count++;
                        }
                    }
                }
            }

            if (count == 1)
            {
                //strComments += "<br /><img src=\"/images/comment.jpg\"><b><a href=\"" + url + "\">" + count.ToString() + " comment</a></b>";
                strComments += string.Format("<br /><span class=\"commentsLink\"><img src=\"/images/comment.jpg\">&nbsp;<b><a href=\"{0}\">{1} comment</a></b></span>", url, count.ToString());
            }
            else if (count > 0)
            {
                //strComments += "<br /><img src=\"/images/comment.jpg\"><b><a href=\"" + url + "\">" + count.ToString() + " comments</a></b>";
                strComments += string.Format("<br /><span class=\"commentsLink\"><img src=\"/images/comment.jpg\">&nbsp;<b><a href=\"{0}\">{1} comments</a></b></span>", url, count.ToString());
            }
            return strComments;
        }
        public List<CommentModel> GetComments(int id)
        {
            if (umbracoHelper == null)
            {
                return new List<CommentModel>();
            }
            var itemNode = umbracoHelper.Content(id);
            
            var commentList = new List<CommentModel>();
            if (itemNode != null)
            {
                var commentContainerId = ContainerId(itemNode.Id);
                var containerNode = umbracoHelper.Content(commentContainerId);

                if (containerNode != null)
                {
                    var containerChildren = containerNode.Children ?? Enumerable.Empty<IPublishedContent>();
                    var nodeComments = containerChildren.Where(i => i?.Parent?.Id == containerNode.Id)
                        .Select(i => new { NodeId = i.Id, userName = pcUtil?.GetContentValue(i, CommentFields.FieldUserName), comment = pcUtil?.GetContentValue(i, CommentFields.FieldComment), i.CreateDate })
                        .Where(i => i != null);

                    if (nodeComments != null)
                    {
                        commentList.AddRange(nodeComments.Select(i => new CommentModel
                        {
                            NodeId = i.NodeId,
                            UserName = i.userName ?? "",
                            Comment = i.comment ?? "",
                            CreateDate = i.CreateDate,
                            DisplayCreateDate = i.CreateDate.ToString(CommentFields.DateFormat)
                        }));
                        commentList = commentList.OrderByDescending(i => i.CreateDate).ToList();
                    }
                }
            }
            return commentList;
        }

        public string CommentFormSubmit(CommentModel commentInfo)
        {
            var message = "<font color=red>An error has occurred while trying to post your comment. Please try again later.</font>";
            
            if (umbracoHelper == null || contentService == null) 
            {
                return message;
            }

            var containerId = ContainerId(commentInfo.NodeId);
            var itemNode = umbracoHelper.Content(commentInfo.NodeId);

            var commentNodeName = commentInfo.UserName.Trim() + " - " + DateTime.Now.ToString(CommentFields.DateFormat);
            var commentDoc = contentService.Create(commentNodeName, containerId, CommentFields.DoctypeComments);

            if (itemNode != null && commentDoc != null)
            {
                commentDoc.SetValue(CommentFields.FieldUserName, commentInfo.UserName.Trim());
                commentDoc.SetValue(CommentFields.FieldComment, commentInfo.Comment.Trim().RemoveHtml());

                var requiresApproval = false;
                var requiresApprovalProp = itemNode.GetProperty(CommentFields.FieldRequiresApproval);
                if (requiresApprovalProp != null)
                {
                    requiresApproval = pcUtil?.GetNodeBoolean(itemNode, CommentFields.FieldRequiresApproval) ?? false;
                }
                if (requiresApproval == false)
                {
                    contentService.SaveAndPublish(commentDoc);
                }
                else
                {
                    //requires approval
                    contentService.Save(commentDoc);
                }

                message = requiresApproval == false ? "<font color=green>Your comment has been posted!</font><br /><br />" :
                    "<font color=green>Your comment will be posted upon approval.</font><br /><br />";
            }

            return message;
        }
    }
}
