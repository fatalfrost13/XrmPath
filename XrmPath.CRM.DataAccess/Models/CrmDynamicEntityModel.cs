using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;


namespace XrmPath.CRM.DataAccess.Models
{
    public class CrmDynamicEntityModel
    {
        public Guid Id {get; set; }   //Id of the record being updated
        public string Name {get;set; }  //Entity name (alias)
        public List<CrmDynamicEntityField> FieldList {get; set; } = new List<CrmDynamicEntityField>();
        public AttributeCollection OriginalFieldList { get; set; } = new AttributeCollection();
    }

    public class CrmDynamicEntityField
    {
        public string Name { get; set; }    //Entity field name (alias)
        public string Type { get; set; } = "String";
        public object Value {get; set; }    //Entity value
        public string RegardingEntity { get; set; }
    }


    /// <summary>
    /// Tag the model with an EntityName
    /// This entity name should match the entity name in CRM
    /// </summary>
    public class CrmDynamicEntityAttribute : Attribute
    {
        private string _entityName = null;
        public virtual string EntityName
        {
            get { return _entityName; }
            set { _entityName = value; }
        }
    }

    /// <summary>
    /// Attributes you can set to a field to perform custom actions when saving to CRM.
    /// 'Type' will force the value to be saved as a specific data type in CRM (String, Boolean, OptionSetValue, Guid, Double)
    /// 'EntityFieldName' should match the entity field name in CRM. If left blank, it will use the name assigned via JsonPropertyAttribute
    /// 'SkipIfValueEquals will skip saving this field is equal do the assigned value.
    /// 'IsPrimaryKey' will flag the field as the primary id field for lookup
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class CrmDynamicEntityFieldAttribute : Attribute
    {
        private string _entityFieldName = null;
        private string _fieldtype = null;
        private string _skipIfValueEquals = null;
        private string _regardingObjectEntity = null;
        private bool _isPrimaryKey = false;
        private bool _readOnly = false;
        public virtual string Type
        {
            get { return _fieldtype; }
            set { _fieldtype = value; }
        }
        public virtual string EntityFieldName
        {
            get { return _entityFieldName; }
            set { _entityFieldName = value; }
        }
        public virtual string SkipSaveIfValueEquals
        {
            get { return _skipIfValueEquals; }
            set { _skipIfValueEquals = value; }
        }
        public virtual bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { _isPrimaryKey = value; }
        }
        public virtual bool ReadOnly
        {
            get { return _readOnly; }
            set { _readOnly = value; }
        }
        public virtual string RegardingObjectEntity
        {
            get { return _regardingObjectEntity; }
            set { _regardingObjectEntity = value; }
        }
    }

    /*Below is an example of what you would pass in to add a contact*/
    /*****Start of example*****/
    /*
     [{
	    "Id": "00000000-0000-0000-0000-000000000000",
	    "Name": "contact",
	    "FieldList": [{
		    "Name": "firstname",
		    "Type": "String",
		    "Value": "Terence"
	    }, {
		    "Name": "lastname",
		    "Type": "String",
		    "Value": "Jee"
	    }]
    }]
    */
    /*****End of example*****/
}
