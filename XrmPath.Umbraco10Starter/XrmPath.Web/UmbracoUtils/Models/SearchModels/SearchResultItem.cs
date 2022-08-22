using System.Runtime.Serialization;

namespace XrmPath.UmbracoUtils.Models
{

    [DataContract]
    public struct SearchResultItem
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public decimal Score { get; set; }
        [DataMember]
        public decimal OriginalScore { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string ShortDescription { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public List<SearchScore> SubScores { get; set; }
    }
}