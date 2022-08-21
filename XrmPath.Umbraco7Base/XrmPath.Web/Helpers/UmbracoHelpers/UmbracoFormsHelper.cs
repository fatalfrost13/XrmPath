using System;
using System.IO;
using System.Linq;
using XrmPath.Helpers.Utilities;
using XrmPath.Web.Helpers.Utils;
using Umbraco.Core.Logging;
using Umbraco.Forms.Core;

namespace XrmPath.Web.Helpers.UmbracoHelpers
{
    public static class UmbracoFormsHelper
    {
        public static Field GetFormField(Form form, Guid fieldId)
        {
            try
            {
                foreach (var p in form.Pages)
                {
                    foreach (var fs in p.FieldSets)
                    {
                        foreach (var c in fs.Containers)
                        {
                            if (c.Fields.Any(x => x.Id == fieldId))
                                return c.Fields.First(x => x.Id == fieldId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on UmbracoFormsHelper.GetFormField(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return null;
        }

        public static string GetFormIdByName(string formName)
        {
            var formId = string.Empty;
            try
            {
                var fs = new Umbraco.Forms.Data.Storage.FormStorage();
                foreach (var f in fs.GetAllForms())
                {
                    if (f.Name == formName)
                    {
                        formId = f.Id.ToString();
                        return formId;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<string>($"XrmPath.Web caught error on UmbracoFormsHelper.GetFormIdByName(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return formId;
        }

        public static bool FormExists(Guid formId)
        {
            //only do check if internally checking form
            var rootUrl = SiteUrlUtility.RootUrl();
            if (!string.IsNullOrEmpty(rootUrl))
            {
                return true;
            }

            var exists = false;
            var formPath = $"/App_Plugins/UmbracoForms/Data/forms/{formId}.json";
            var file = System.Web.Hosting.HostingEnvironment.MapPath(formPath);
            if (file != null)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    exists = true;
                }
            }
            return exists;
        }

        public static string GenerateUniqueId(string Id)
        {
            var uniqueId = Guid.NewGuid().ToString().Replace("-","");
            var umbracoFormsId = $"UID{uniqueId}_{Id}";
            return umbracoFormsId;
        }
    }
}