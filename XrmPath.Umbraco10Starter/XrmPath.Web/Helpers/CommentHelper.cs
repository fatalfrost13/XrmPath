using Umbraco.Cms.Core.Models;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Utilities;
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

        public string AliasCategory = "category";
        public string FieldAllowComments = "allowComments";
        public string FieldRequiresApproval = "requiresApproval";
        public string FieldUserName = "userName";
        public string FieldComment = "userComment";
        public string FieldHidden = "umbracoNaviHide";
        public string NameCommentsContainer = "Comments";
        public string DoctypeContainer = "commentsContainer";
        public string DoctypeComments = "comment";
        public string DateFormat = "MMMM d, yyyy h:mm tt";

        public int ContainerId(int listItemId)
        {
            //int[] containerId = { 0 };
            var containerId = 0;
            //var listNode = new Node(listItemId);
            var listNode = umbracoHelper?.Content(listItemId);

            if (listNode?.Id > 0)
            {
                var container = listNode?.Children?.FirstOrDefault(i => i.ContentType.Alias.Equals(DoctypeContainer));
                //foreach (var container in from IPublishedContent container in listNode.Children where containerId[0] == 0 where container.DocumentTypeAlias.ToLower() == DoctypeContainer select container)
                //{
                //    containerId[0] = container.Id;
                //}

                if (container == null && listNode != null)
                {
                    //no container. need to create a container
                    var containerDoc = contentService?.Create(NameCommentsContainer, listNode.Id, DoctypeContainer);
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

            var node = umbracoHelper?.Content(nodeid);
            if (pcUtil?.NodeExists(node) ?? false)
            {
                var allowComments = false;
                var allowCommentsProp = node?.GetProperty(FieldAllowComments);
                if (allowCommentsProp != null)
                {
                    allowComments = pcUtil?.GetNodeBoolean(node, FieldAllowComments) ?? false;
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

            if (ShowComments(nodeid))
            {
                var containerId = ContainerId(nodeid);
                var containerNode = umbracoHelper?.Content(containerId);
                if (containerNode != null)
                {
                    foreach (var commentNode in containerNode?.Children)
                    {
                        var hidden = false;
                        var hiddenProp = commentNode.GetProperty(FieldHidden);
                        if (hiddenProp != null)
                        {
                            hidden = pcUtil?.GetNodeBoolean(commentNode, FieldHidden) ?? false;
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
            var itemNode = umbracoHelper?.Content(id);
            var commentList = new List<CommentModel>();
            if (itemNode != null)
            {
                var commentContainerId = ContainerId(itemNode.Id);
                var containerNode = umbracoHelper?.Content(commentContainerId);

                if (containerNode != null)
                {
                    var nodeComments = containerNode?.Children.Where(i => i?.Parent?.Id == containerNode.Id)
                        .Select(i => new { NodeId = i.Id, userName = pcUtil?.GetContentValue(i, FieldUserName), comment = pcUtil?.GetContentValue(i, FieldComment), i.CreateDate });
                    if (nodeComments != null)
                    {
                        commentList.AddRange(nodeComments.Select(i => new CommentModel
                        {
                            NodeId = i.NodeId,
                            UserName = i?.userName ?? "",
                            Comment = i?.comment ?? "",
                            CreateDate = i.CreateDate,
                            DisplayCreateDate = i.CreateDate.ToString(DateFormat)
                        }));
                        commentList = commentList.OrderByDescending(i => i.CreateDate).ToList();
                    }
                }
            }
            return commentList;
        }

        public string CommentFormSubmit(CommentModel commentInfo)
        {
            string message = "<font color=red>An error has occurred while trying to post your comment. Please try again later.</font>";

            var containerId = ContainerId(commentInfo.NodeId);
            var itemNode = umbracoHelper?.Content(commentInfo.NodeId);

            var commentNodeName = commentInfo.UserName.Trim() + " - " + DateTime.Now.ToString(DateFormat);
            var commentDoc = contentService?.Create(commentNodeName, containerId, DoctypeComments);

            if (itemNode != null && commentDoc != null)
            {
                commentDoc.SetValue(FieldUserName, commentInfo.UserName.Trim());
                commentDoc.SetValue(FieldComment, commentInfo.Comment.Trim().RemoveHtml());

                var requiresApproval = false;
                var requiresApprovalProp = itemNode.GetProperty(FieldRequiresApproval);
                if (requiresApprovalProp != null)
                {
                    requiresApproval = pcUtil?.GetNodeBoolean(itemNode, FieldRequiresApproval) ?? false;
                }
                if (requiresApproval == false)
                {
                    contentService?.SaveAndPublish(commentDoc);
                }
                else
                {
                    //requires approval
                    contentService?.Save(commentDoc);
                }

                message = requiresApproval == false ? "<font color=green>Your comment has been posted!</font><br /><br />" :
                    "<font color=green>Your comment will be posted upon approval.</font><br /><br />";
            }

            return message;
        }
    }
}
