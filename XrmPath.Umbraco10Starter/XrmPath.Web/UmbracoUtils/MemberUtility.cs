using Umbraco.Cms.Core.Models;
using XrmPath.Helpers.Utilities;

namespace XrmPath.UmbracoUtils
{
    public static class MemberUtility
    {
        //private IMember _iMember;
        //public MemberUtility(IMember _member)
        //{
        //    if (_iMember == null)
        //    {
        //        _iMember = _member;
        //    }
        //}

        public static string GetMemberValue(this IMember member, string propertyAlias, string defaultValue = "")
        {
            var result = defaultValue;
            try
            {
                if (member != null && member.Id > 0 && member.HasProperty(propertyAlias))
                {
                    var fieldValue = member.GetValue(propertyAlias)?.ToString();
                    if (!string.IsNullOrEmpty(fieldValue))
                    {
                        result = fieldValue;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValue(). URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValue(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return result;
        }

        public static bool GetMemberValueBoolean(this IMember member, string alias)
        {
            var boolValue = false;
            try
            {
                var contentValue = member.GetMemberValue(alias);
                boolValue = StringUtility.ToBoolean(contentValue);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValueBoolean(). URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValueBoolean(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return boolValue;
        }

        public static int GetMemberValueInt(this IMember member, string alias)
        {
            var intValue = 0;
            try
            {
                var contentValue = member.GetMemberValue(alias);

                if (!string.IsNullOrEmpty(contentValue))
                {
                    int.TryParse(contentValue, out intValue);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValueInt(). URL Info: {UrlUtility.GetCurrentUrl()}");
                //LogHelper.Error($"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValueInt(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return intValue;
        }
    }
}
