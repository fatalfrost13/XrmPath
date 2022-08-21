using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.CRM.DataAccess.Models
{
    public class OptionSetModel
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
    public class LookupItem
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
    }

    public class LookupItemCache
    {
        public List<LookupItem> LookupItemList = new List<LookupItem>();
        public DateTime DateCached { get; set; } = DateTime.UtcNow;
    }

    public static class GlobalBooleanOptionSet
    {
        //-1 == false || 4 == true || -2147483648 == unassigned
        public static int UnAssigned = -2147483648;
        public static int False = 0;
        public static int True = 4;
    }
}
