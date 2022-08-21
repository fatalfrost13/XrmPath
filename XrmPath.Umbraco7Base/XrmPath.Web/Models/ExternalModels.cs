using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XrmPath.Web.Models
{
    public class ExternalModels
    {
    }

    public class AlbertaCaNavigation
    {
        public string IndexPage { get; set; }
        public DateTime DateLoaded { get; set; } = DateTime.UtcNow;
    }

    public class ExternalWebContent
    {
        public string WebContent { get; set; }
        public string Url {get; set; }
        public DateTime DateLoaded { get; set; } = DateTime.UtcNow;
    }
}