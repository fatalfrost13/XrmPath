namespace XrmPath.UmbracoCore.Models
{
    using Umbraco.Cms.Core.Models;

    public class UrlPicker
    {
        public string? Title { get; set; } = "";
        public string? Caption { get; set; } = "";
        public LinkType LinkType { get; set; } = LinkType.Content;  //0=Content, 1=Media, 2=External
        public int NodeId { get; set; } = 0;
        public string? Url { get; set; } = "";
        public bool NewWindow { get; set; } = false;
    }

    public class LinkModel
    {
        public string? Url { get; set; } = "";
        public string? Name { get; set; } = "";
        public int Id { get; set; } = 0;
        public LinkType Type { get; set; } = LinkType.Content;
        public string? Target { get; set; } = "_self";
    }
}