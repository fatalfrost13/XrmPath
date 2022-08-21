using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmPath.CRM.DataAccess.Models
{
    [CrmDynamicEntity(EntityName = "task")]
    public class TaskModel
    {
        [CrmDynamicEntityField(Type = "Guid", IsPrimaryKey = true, ReadOnly = true)]
        public Guid Id { get; set; }
        [CrmDynamicEntityField(EntityFieldName = "subject", Type = "String")]
        public string Name { get; set; }
        [CrmDynamicEntityField(EntityFieldName = "description", Type = "String")]
        public string Description { get; set; }
        [CrmDynamicEntityField(EntityFieldName = "regardingobjectid", Type = "Guid", RegardingObjectEntity = "incident")]
        public Guid? RegardingObjectIdCase { get; set; }
    }
}