using System.Runtime.Serialization;

namespace XrmPath.UmbracoCore.Models
{

    [DataContract]
    public struct SearchResultItemPager
    {
        public SearchResultItemPager() { }
        [DataMember]
        public List<SearchResultItem> SearchResultItems { get; set; } = new List<SearchResultItem>();
        [DataMember]
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }

}