using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Umbraco.Core.Services;
using Umbraco.Web;
using XrmPath.Helpers.Utilities;
using XrmPath.UmbracoCore.Helpers;

namespace XrmPath.UmbracoCore.Utilities
{
    public static class MembershipUtility
    {
        
        public static readonly string WebMembershipApplicationName = "Demo";
        //public static readonly string UserNodeAccessCookieValue = "UserNodeAccess";
        //public static readonly int UserNodeAccessExpiryHours = 6;

        public static void CustomSignout()
        {
            FormsAuthentication.SignOut();
        }

        public static void ExpireCookie(string cookieName = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(cookieName))
                {
                    HttpContext.Current.Request.Cookies.Remove(cookieName);
                    var httpCookie = HttpContext.Current.Response.Cookies[cookieName];
                    if (httpCookie != null)
                    {
                        httpCookie.Value = null;
                        var expiredCookie = new HttpCookie(httpCookie.Name)
                        {
                            Expires = DateTime.Now.AddDays(-1)
                        };
                        HttpContext.Current.Response.Cookies.Add(expiredCookie);
                    }
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MembershipUtility.ExpireCookie(). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MembershipUtility.ExpireCookie(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
        }

        public static void CreateCookie(string cookieName = null, string userData = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(cookieName) && !string.IsNullOrEmpty(userData))
                {
                    ExpireCookie(cookieName); //check if cookie exists and exire it if it does
                    var userCookie = new HttpCookie(cookieName)
                    {
                        Value = userData
                    };
                    HttpContext.Current.Response.Cookies.Add(userCookie);
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MembershipUtility.CreateCookie(). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MembershipUtility.CreateCookie(). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }
        }

        /// <summary>
        /// Loads restricted site access Ids to the UserInfo cookie
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static List<int> AccessIds(string email)
        {
            var accessIds = new List<int>();
            try
            {
                var currentUser = Membership.GetUser(email);
                if (currentUser != null)
                {
                    var rootNodes = ServiceUtility.UmbracoHelper.ContentAtRoot();
                    //var accessIds = rootNodes.SelectMany(i => i.FindAllNodes(UmbracoCustomTypes.TileContentTypes.StringToSet())).Where(i => ServiceUtility.UmbracoHelper.IsProtected(i.Path) && ServiceUtility.PublicAccessService.HasAccess(i.Path, currentUser, Roles.Provider)).Select(i => i.Id).ToList();
                    accessIds = rootNodes.SelectMany(r => r.DescendantsOrSelf().Where(i => ServiceUtility.PublicAccessService.IsProtected(i.Path) && ServiceUtility.PublicAccessService.HasAccess(i.Path, currentUser, Roles.Provider)).Select(i => i.Id)).ToList();
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, $"XrmPath.UmbracoCore caught error on MembershipUtility.AccessIds({email}). URL Info: {UrlUtility.GetCurrentUrl()}");
                LogHelper.Error($"XrmPath.UmbracoCore caught error on MembershipUtility.AccessIds({email}). URL Info: {UrlUtility.GetCurrentUrl()}", ex);
            }

            return accessIds;
        }

        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }
}