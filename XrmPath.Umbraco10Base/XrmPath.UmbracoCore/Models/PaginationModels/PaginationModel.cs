namespace XrmPath.UmbracoCore.Models.PaginationModels
{

    public struct PaginationModel
    {
        public int TotalItemCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int NumberOfPages { get; set; }
        public bool ValidCurrentPage { get; set; }
        public bool ValidPageSize { get; set; }
        public int IndexStart { get; set; }
        public int IndexEnd { get; set; }
    }
}
