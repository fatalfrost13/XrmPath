using Umbraco.Cms.Core.Models;
using XrmPath.Helpers.Utilities;

namespace XrmPath.UmbracoCore.Utilities
{
    public class MemberUtility: BaseInitializer
    {
        /// <summary>
        /// Dependencies: Logger(optional)
        /// </summary>
        /// <param name="serviceUtil"></param>
        public MemberUtility(ServiceUtility? serviceUtil) : base(serviceUtil) { }
        //private IMember _iMember;
        //public MemberUtility(IMember _member)
        //{
        //    if (_iMember == null)
        //    {
        //        _iMember = _member;
        //    }
        //}

        public string GetMemberValue(IMember? member, string propertyAlias, string defaultValue = "")
        {
            var result = defaultValue;

            if (member == null) {
                return result;
            }

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
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValue(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return result;
        }

        public bool GetMemberValueBoolean(IMember? member, string alias)
        {
            var boolValue = false;
            if (member == null)
            {
                return boolValue;
            }
            try
            {
                var contentValue = GetMemberValue(member, alias);
                boolValue = StringUtility.ToBoolean(contentValue);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValueBoolean(). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValueBoolean(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return boolValue;
        }

        public int GetMemberValueInt(IMember? member, string alias)
        {
            var intValue = 0;
            if (member == null)
            {
                return intValue;
            }
            try
            {
                var contentValue = GetMemberValue(member, alias);

                if (!string.IsNullOrEmpty(contentValue))
                {
                    int.TryParse(contentValue, out intValue);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValueInt(). URL Info: {UrlUtility.GetCurrentUrl()}");
                loggingUtil?.Error($"XrmPath.UmbracoCore caught error on MemberUtility.GetMemberValueInt(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
            return intValue;
        }
    }
}
