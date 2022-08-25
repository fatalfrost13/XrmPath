namespace XrmPath.Web.Models
{
    public class WebPageModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? TileBackground { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string? Body { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
    }
}
