using System;
using System.Collections.Generic;

namespace XrmPath.Web.Models
{
    public class WebUserModel
    {
        public Guid? Id { get; set; }
        public string Email { get; set; }
        public List<int> VisitedIds { get; set; } = new List<int>();
        public List<int> FavoriteIds { get; set; } = new List<int>();
        public List<int> AccessIds { get; set; } = new List<int>();
    }

    public class UserNodeAccessModel
    {
        public string Email { get; set; }
        public List<int> AccessIds { get; set; } = new List<int>();
        public DateTime DateLoaded { get; set; } = DateTime.UtcNow;
    }
}