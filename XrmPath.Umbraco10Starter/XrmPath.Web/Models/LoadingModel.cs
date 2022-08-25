namespace XrmPath.Web.Models
{
    public class LoadingModel
    {
        public int Type { get; set; } = 0;
        public string LoadingId { get; set; } = Guid.NewGuid().ToString();
        public string? LoadingMessage { get; set; } = "";
        public string? LoadingStyle { get; set; } = "";
        public string LoadingClass { get; set; } = "loadingArea";
    }
}
