using System;

namespace XrmPath.UmbracoUtils.Models
{
    public static class PaginationExtensions
    {
        public static PaginationModel PaginationValues(int pagesize, int currentpage, int resultCount)
        {
            int pageSize;
            int numberOfPages;
            int currentPage;
            var indexStart = -1;
            var indexEnd = -1;
            var validPageSize = pagesize > 0;
            var validCurrentPage = currentpage > 0;


            if (validPageSize && validCurrentPage)
            {
                //calculate start and end index
                indexStart = (pagesize * currentpage) - pagesize;
                indexEnd = indexStart + (pagesize - 1);

                pageSize = pagesize;
                numberOfPages = resultCount > 0 ? (int)Math.Ceiling(resultCount / (double)pageSize) : 1;
                currentPage = currentpage;
            }
            else
            {
                pageSize = 0;
                numberOfPages = 0;
                currentPage = 0;
            }

            var paginationValues = new PaginationModel
            {
                TotalItemCount = resultCount,
                CurrentPage = currentPage,
                NumberOfPages = numberOfPages,
                PageSize = pageSize,
                ValidCurrentPage = validCurrentPage,
                ValidPageSize = validPageSize,
                IndexStart = indexStart,
                IndexEnd = indexEnd
            };
            return paginationValues;
        }
    }
}
