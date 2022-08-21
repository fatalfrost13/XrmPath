using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.UmbracoCore.Models.SearchModels
{
    public class SearchScore
    {
        public int NodeId { get; set; }
        public decimal Score { get; set; }
        public decimal OriginalScore { get; set; }
    }
}
