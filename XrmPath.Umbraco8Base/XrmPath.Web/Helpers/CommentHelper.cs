using System;
using System.Collections.Generic;
using System.Linq;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Utilities;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Helpers
{

    public static class CommentHelper
    {
        //static readonly ContentService contentService = ServiceUtility.ContentService;

        public static string AliasCategory = "category";
        public static string FieldAllowComments = "allowComments";
        public static string FieldRequiresApproval = "requiresApproval";
        public static string FieldUserName = "userName";
        public static string FieldComment = "userComment";
        public static string FieldHidden = "umbracoNaviHide";
        public static string NameCommentsContainer = "Comments";
        public static string DoctypeContainer = "commentsContainer";
        public static string DoctypeComments = "comment";
        public static string DateFormat = "MMMM d, yyyy h:mm tt";


        /// <summary>
        /// Global Format Bool method
        /// Will also accept "1", "yes", or "true" and return true. Else will return false.
        /// </summary>
        /// <param name="obj">value to attempt bool format</param>
        /// <returns>boolean value (false if error or null)</returns>
        public static bool FormatBool(this object obj)
        {
            bool blValue;
            var isBool = bool.TryParse(obj.ToString(), out blValue);

            if (!isBool)
            {
                var strValue = obj.ToString();
                strValue = strValue.ToLower().Trim();
                if (strValue == "1" || strValue == "yes" || strValue == "true")
                {
                    blValue = true;
                }
            }
            return blValue;
        }


        public static int ContainerId(this int listItemId)
        {
            //int[] containerId = { 0 };
            var containerId = 0;
            //var listNode = new Node(listItemId);
            var listNode = ServiceUtility.UmbracoHelper.GetById(listItemId);

            if (listNode.Id > 0)
            {
                var container = listNode.Children.FirstOrDefault(i => i.ContentType.Alias.Equals(DoctypeContainer));
                //foreach (var container in from IPublishedContent container in listNode.Children where containerId[0] == 0 where container.DocumentTypeAlias.ToLower() == DoctypeContainer select container)
                //{
                //    containerId[0] = container.Id;
                //}

                if (container == null)
                {
                    //no container. need to create a container
                    var containerDoc = ServiceUtility.ContentService.Create(NameCommentsContainer, listNode.Id, DoctypeContainer);
                    //containerDoc.SetValue("title", NameCommentsContainer);
                    ServiceUtility.ContentService.SaveAndPublish(containerDoc);
                    containerId = containerDoc.Id;
                }
                else {
                    containerId = container.Id;
                }
            }
            return containerId;
        }

        public static bool ShowComments(this int nodeid)
        {
            var showComments = false;

            var node = ServiceUtility.UmbracoHelper.GetById(nodeid);
            if (node.NodeExists())
            {
                var allowComments = false;
                var allowCommentsProp = node.GetProperty(FieldAllowComments);
                if (allowCommentsProp != null)
                {
                    allowComments = node.GetNodeBoolean(FieldAllowComments);
                }

                if (allowComments)
                {
                    //show comments
                    showComments = true;
                }
            }
            return showComments;
        }

        public static string ViewComments(this int nodeid, string url)
        {
            var strComments = "";
            var count = 0;

            if (ShowComments(nodeid))
            {
                var containerId = ContainerId(nodeid);
                var containerNode = ServiceUtility.UmbracoHelper.GetById(containerId);
                foreach (var commentNode in containerNode.Children)
                {
                    var hidden = false;
                    var hiddenProp = commentNode.GetProperty(FieldHidden);
                    if (hiddenProp != null)
                    {
                        hidden = commentNode.GetNodeBoolean(FieldHidden);
                    }

                    if (hidden == false)
                    {
                        count++;
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

        public static List<CommentModel> GetComments(int id)
        {
            var itemNode = ServiceUtility.UmbracoHelper.GetById(id);
            var commentList = new List<CommentModel>();
            var commentContainerId = CommentHelper.ContainerId(itemNode.Id);
            var containerNode = ServiceUtility.UmbracoHelper.GetById(commentContainerId);

            var nodeComments = containerNode.Children.Where(i => i.Parent.Id == containerNode.Id)
                .Select(i => new { NodeId = i.Id, userName = i.GetContentValue(CommentHelper.FieldUserName), comment = i.GetContentValue(CommentHelper.FieldComment), i.CreateDate });
            commentList.AddRange(nodeComments.Select(i => new CommentModel
            {
                NodeId = i.NodeId,
                UserName = i.userName,
                Comment = i.comment,
                CreateDate = i.CreateDate,
                DisplayCreateDate = i.CreateDate.ToString(CommentHelper.DateFormat)
            }));
            commentList = commentList.OrderByDescending(i => i.CreateDate).ToList();
            return commentList;
        }

        public static string CommentFormSubmit(CommentModel commentInfo)
        {
            var containerId = commentInfo.NodeId.ContainerId();
            var itemNode = ServiceUtility.UmbracoHelper.GetById(commentInfo.NodeId);

            var commentNodeName = commentInfo.UserName.Trim() + " - " + DateTime.Now.ToString(CommentHelper.DateFormat);
            var commentDoc = ServiceUtility.ContentService.Create(commentNodeName, containerId, CommentHelper.DoctypeComments);
            commentDoc.SetValue(CommentHelper.FieldUserName, commentInfo.UserName.Trim());
            commentDoc.SetValue(CommentHelper.FieldComment, commentInfo.Comment.Trim().RemoveHtml());


            var requiresApproval = false;
            var requiresApprovalProp = itemNode.GetProperty(CommentHelper.FieldRequiresApproval);
            if (requiresApprovalProp != null)
            {
                requiresApproval = itemNode.GetNodeBoolean(CommentHelper.FieldRequiresApproval);
            }
            if (requiresApproval == false)
            {
                ServiceUtility.ContentService.SaveAndPublish(commentDoc);
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

    }

}