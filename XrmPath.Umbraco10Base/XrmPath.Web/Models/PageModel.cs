using System.Collections.Generic;

namespace XrmPath.UmbracoCore.Models
{
    public class PageModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TileBackground { get; set; }
        public List<string> Tags { get; set; }
        public string Body { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
    }
}