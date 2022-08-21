using System;

namespace XrmPath.UmbracoCore.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public struct CommentModel
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