using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.Helpers.Model
{
    public class ApiPostModel
    {
        public string Name { get; set; } = "";
        public string Id { get; set; } = "";
        public List<ApiPostAttributes> Attributes { get; set;} = new List<ApiPostAttributes>();
    }
    public class ApiPostAttributes
    {
        public string Name {get; set; } = "";
        public string Value {get;set; } = "";
    }
}
