using System.Collections.Generic;
using System.Runtime.Serialization;
using XrmPath.UmbracoCore.Models.PaginationModels;

namespace XrmPath.UmbracoCore.SearchModels
{

    [DataContract]
    public struct SearchResultItemPager
    {
        [DataMember]
        public List<SearchResultItem> SearchResultItems { get; set; }
        [DataMember]
        public PaginationModel Pagination { get; set; }
    }

}