using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;

namespace XrmPath.UmbracoCore.Models
{

    [DataContract]
    public struct SearchResultItem
    {
        public SearchResultItem() { }

        [DataMember]
        public int Id { get; set; } = 0;
        [DataMember]
        public decimal Score { get; set; } = 0;
        [DataMember]
        public decimal OriginalScore { get; set; } = 0;
        [DataMember]
        public string? Title { get; set; } = "";
        [DataMember]
        public string? ShortDescription { get; set; } = "";
        [DataMember]
        public string? Url { get; set; } = "";
        [DataMember]
        public string? Type { get; set; } = "";
        [DataMember]
        public List<SearchScore> SubScores { get; set; } = new List<SearchScore>();
    }
}