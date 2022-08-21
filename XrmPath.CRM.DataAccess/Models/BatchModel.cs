using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.CRM.DataAccess.Models
{
    public class BatchModel
    {
        public List<Entity> EntityList { get; set; } = new List<Entity>();
    }
}
