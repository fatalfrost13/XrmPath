using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmPath.CRM.DataAccess.Helpers;
using XrmPath.CRM.DataAccess.Model;
using XrmPath.CRM.DataAccess.Models;

namespace XrmPath.CRM.DataAccess.Sample
{
    /// <summary>
    /// Saving: If Id is null or Guid.Empty, it will perform a create/insert. If Id has a valid Guid, it will perform an update
    /// </summary>
    public static class ProductHelper
    {
        public static string EntityName = "new_product";
        public static string Columns = "new_productid,new_name,new_description,new_status,createdon";
        public static List<Product> GetProductList() 
        {
            var entityList = GetDynamicProductList().Select(i => i.ToModel<Product>()).ToList();
            return entityList;
        }

        public static Product GetSingleProduct(Guid Id) 
        {
            var entity = GetDynamicProduct(Id).ToModel<Product>();

            //try convert back to dynamic object
            //var dynamic = TransformHelper.TransformToEntity(entity);

            return entity;
        }

        public static List<CrmDynamicEntityModel> GetDynamicProductList() 
        {

            //you can add filter expressions to filter by specific criteria using the code below
            var filterExpression = new FilterExpression();
            filterExpression.AddCondition("new_status", ConditionOperator.Equal, ProductLookup.Status.Active);

            //you can add sort expressions to sort by specific criteria using the code below
            var orderExpression = new OrderExpression { OrderType = OrderType.Descending, AttributeName = "createdon" };

            var entityList = CrmDynamicEntityHelper.GetDynamicEntityList(EntityName, Columns, filterExpression, orderExpression);
            return entityList;
        }

        public static CrmDynamicEntityModel GetDynamicProduct(Guid Id) 
        {
            var entity = CrmDynamicEntityHelper.GetDynamicEntity(EntityName, Id, Columns);
            return entity;
        
        }

        public static MessageModel SaveProduct(Product productModel)
        {
            var message = new MessageModel();
            try
            {
                message = CrmDynamicEntityHelper.SaveEntityToCrm(productModel);
            }
            catch (Exception ex)
            {
                message.Error = true;
                message.Message = $"An error has occurred: {ex}";

            }
            return message;
        }

        public static MessageModel SaveProductList(List<Product> productModelList)
        {
            var message = new MessageModel();
            try
            {
                //this will also leverage saving batch of records (as opposed to individual requests)
                message = CrmDynamicEntityHelper.SaveEntitiesToCrm(productModelList);
            }
            catch (Exception ex)
            {
                message.Error = true;
                message.Message = $"An error has occurred: {ex}";

            }
            return message;
        }

        public static MessageModel SaveProductDynamic(CrmDynamicEntityModel productModel)
        {
            var message = new MessageModel();
            try
            {
                message = CrmDynamicEntityHelper.SaveDynamicEntity(productModel);
            }
            catch (Exception ex)
            {
                message.Error = true;
                message.Message = $"An error has occurred: {ex}";

            }
            return message;
        }

        public static MessageModel SaveProductListDynamic(List<CrmDynamicEntityModel> productModelList)
        {
            var message = new MessageModel();
            try
            {
                message = CrmDynamicEntityHelper.SaveDynamicEntities(productModelList);
            }
            catch (Exception ex)
            {
                message.Error = true;
                message.Message = $"An error has occurred: {ex}";

            }
            return message;
        }
    }
}
