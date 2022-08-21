using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmPath.CRM.DataAccess.Models;

namespace XrmPath.CRM.DataAccess.Sample
{
    [CrmDynamicEntityAttribute(EntityName = "new_product")]
    public class ProductModel
    {
        [JsonProperty("new_productid")]
        [CrmDynamicEntityField(Type = "Guid", EntityFieldName = "new_productid", IsPrimaryKey = true, ReadOnly = true)]
        public Guid Id { get; set; }

        [JsonProperty("new_name")]
        [CrmDynamicEntityField(Type = "String", EntityFieldName = "new_name")]

        public string ProductName { get; set; }
        [JsonProperty("new_description")]
        [CrmDynamicEntityField(Type = "String", EntityFieldName = "new_description")]
        public string ProductDescription { get; set; }
        public decimal Cost { get; set; }

        [JsonProperty("new_status")]
        [CrmDynamicEntityField(Type = "OptionSetValue", EntityFieldName = "new_status")]

        public int? ProductStatus { get; set; }

        [JsonProperty("createdon")]
        [CrmDynamicEntityField(Type = "DateTime", EntityFieldName = "createdon")]
        public DateTime? CreatedOn { get; set; }

        //below is if you have a parent category entity reference named "category".
        //RegardingObjectEntity is the joining entity name
        //EntityFieldName is the field name that joins the entity
        [JsonProperty("parentcategoryid")]
        [CrmDynamicEntityField(Type = "EntityReference", EntityFieldName = "parentcategoryid", RegardingObjectEntity = "category")]
        public string CategoryId { get; set; }
    }
}
