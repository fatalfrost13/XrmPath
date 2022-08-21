using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;
using XrmPath.Web.Helpers.UmbracoHelpers;

namespace XrmPath.Web.Helpers
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
        /// Global function to format an object to string.
        /// Error handling so if a value is null will return empty string
        /// </summary>
        /// <param name="objValue">object value</param>
        /// <returns>converted object value to string</returns>
        public static string FormatString(this object objValue)
        {
            var strValue = "";

            if (objValue != null && objValue.ToString() != "")
            {
                strValue = objValue.ToString().Trim();
            }

            return strValue;
        }


        /// <summary>
        /// Global Format Integer method 
        /// Catches invalid or null values and returns 0 for those values.
        /// </summary>
        /// <param name="obj">value to attempt int format</param>
        /// <returns>integer value (0 if error or null)</returns>
        public static int FormatInteger(this object obj)
        {
            int num;
            Int32.TryParse(obj.ToString(), out num);
            //num = Int32.Parse(obj.ToString().Trim()); 
            return num;
        }

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
            var umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();
            //var listNode = new Node(listItemId);
            var listNode = umbracoHelper.GetById(listItemId);

            if (listNode.Id > 0)
            {
                var container = listNode.Children.FirstOrDefault(i => i.DocumentTypeAlias.Equals(DoctypeContainer));
                //foreach (var container in from IPublishedContent container in listNode.Children where containerId[0] == 0 where container.DocumentTypeAlias.ToLower() == DoctypeContainer select container)
                //{
                //    containerId[0] = container.Id;
                //}

                if (container == null)
                {
                    //no container. need to create a container
                    var containerDoc = ServiceUtility.ContentService.CreateContent(NameCommentsContainer, listNode.Id, DoctypeContainer);
                    //containerDoc.SetValue("title", NameCommentsContainer);
                    ServiceUtility.ContentService.SaveAndPublishWithStatus(containerDoc);
                    containerId = containerDoc.Id;
                }
                else {
                    containerId = container.Id;
                }
            }
            return containerId;
        }

        public static int ContainerIdByAlias(this IPublishedContent node, string alias)
        {
            var blExit = false;
            var nodeId = 0;
            var tmpNode = node;

            var umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();

            while (blExit == false)
            {
                if (tmpNode.DocumentTypeAlias == alias)
                {
                    nodeId = tmpNode.Id;
                    blExit = true;
                }
                else
                {
                    tmpNode = umbracoHelper.GetById(tmpNode.Parent.Id);
                    if (!tmpNode.NodeExists())
                    {
                        blExit = true;
                    }
                }
            }
            return nodeId;
        }





        //public static void UpdateDocumentCache(this Document doc)
        //{
        //    umbraco.library.UpdateDocumentCache(doc.Id);
        //    //umbraco.library.RefreshContent();
        //}

        public static bool ShowComments(this int nodeid)
        {
            var umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();
            var showComments = false;

            var node = umbracoHelper.GetById(nodeid);
            if (node.NodeExists())
            {
                var allowComments = false;
                var allowCommentsProp = node.GetProperty(FieldAllowComments);
                if (allowCommentsProp != null)
                {
                    allowComments = FormatBool(node.GetProperty(FieldAllowComments).Value);
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

            var umbracoHelper = CustomUmbracoHelper.GetUmbracoHelper();

            if (ShowComments(nodeid))
            {
                var containerId = ContainerId(nodeid);
                var containerNode = umbracoHelper.GetById(containerId);
                foreach (var commentNode in containerNode.Children)
                {
                    var hidden = false;
                    var hiddenProp = commentNode.GetProperty(FieldHidden);
                    if (hiddenProp != null)
                    {
                        hidden = FormatBool(commentNode.GetProperty(FieldHidden).Value);
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


    }

}