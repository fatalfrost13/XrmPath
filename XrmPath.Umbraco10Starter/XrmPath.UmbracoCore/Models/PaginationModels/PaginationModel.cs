namespace XrmPath.UmbracoCore.Models
{

    public struct PaginationModel
    {
        public PaginationModel() { }
        public int TotalItemCount { get; set; } = 0;
        public int PageSize { get; set; } = 0;
        public int CurrentPage { get; set; } = 0;
        public int NumberOfPages { get; set; } = 0;
        public bool ValidCurrentPage { get; set; } = false;
        public bool ValidPageSize { get; set; } = false;
        public int IndexStart { get; set; } = 0;
        public int IndexEnd { get; set; } = 0;
    }
}
