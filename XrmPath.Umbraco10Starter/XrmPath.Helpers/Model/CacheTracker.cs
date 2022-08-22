using System;

namespace XrmPath.Helpers.Model
{
    public class CacheTracker
    {
        public string FileName { get; set; } = "";
        public DateTime Modified { get; set; } = DateTime.UtcNow;
    }
}