using System.Collections.Generic;

namespace XrmPath.Web.Models
{

    public class LinkItem
    {
        public int NodeId { get; set; }
        public string Title { get; set; }
        public UrlPicker UrlPicker { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Content (0), Media (1), External (2)
        /// </summary>
        public string LinkType { get; set; }
        public string InternalUrl { get; set; }
    }

    public class LinkFolder
    {
        public string FolderName { get; set; }
        public List<LinkItem> LinkList { get; set; }
    }

    public class Links
    {
        public List<LinkFolder> LinkFolders { get; set; }
        public List<LinkItem> LinkList { get; set; }
    }
}