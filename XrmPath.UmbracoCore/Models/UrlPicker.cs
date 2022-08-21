namespace XrmPath.UmbracoCore.Models
{
    using Umbraco.Web.Models;

    public class UrlPicker
    {
        public string Title { get; set; }
        public string Caption { get; set; }
        public LinkType LinkType { get; set; }  //0=Content, 1=Media, 2=External
        public int? NodeId { get; set; }
        public string Url { get; set; }
        public bool NewWindow { get; set; }
    }

    public class LinkModel
    {
        public string Url { get; set; } = string.Empty;
        public string Name { get; set; }
        public int Id { get; set; }
        public LinkType Type { get; set; } 
        public string Target { get; set; }
    }
}