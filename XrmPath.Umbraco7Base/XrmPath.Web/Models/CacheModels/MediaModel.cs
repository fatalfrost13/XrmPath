using System;
using System.Collections.Generic;

namespace XrmPath.Web.Models.CacheModels
{
    public class MediaModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime DateCached { get; set; } = DateTime.UtcNow;
    }

    public class MediaModelList
    {
        public List<MediaModel> MediaList { get; set; } = new List<MediaModel>();
    }
}