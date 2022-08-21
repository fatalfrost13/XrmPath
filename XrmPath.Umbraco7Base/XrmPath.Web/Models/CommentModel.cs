using System;

namespace XrmPath.Web.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public struct CommentItem
    {
        [DataMember]
        public int NodeId { get; set; }
        [DataMember]
        public string ListAlias { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public string DisplayCreateDate { get; set; }
    }
}