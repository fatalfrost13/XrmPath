using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.CRM.DataAccess.Models
{
    public class JoinExpression
    {
        public string LinkToEntityName { get; set; }
        public string LinkFromAttributeName { get; set; }
        public string LinkToAttributeName { get; set; }
        public JoinOperator JoinOperator { get; set; }
        public string Columns { get; set; }
    }

}
