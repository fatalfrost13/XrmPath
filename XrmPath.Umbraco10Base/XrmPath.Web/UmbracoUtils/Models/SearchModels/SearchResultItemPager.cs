using System.Runtime.Serialization;

namespace XrmPath.UmbracoUtils.Models
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