using XrmPath.UmbracoCore.BaseServices;
using XrmPath.UmbracoCore.Models;

namespace XrmPath.UmbracoCore.Utilities
{
    /// <summary>
    /// Dependencies: Logger(optional), MemberSignInManager
    /// </summary>
    /// <param name="serviceUtil"></param>
    public class MembershipUtility: BaseInitializer
    {
        public MembershipUtility(ServiceUtility? serviceUtil) : base(serviceUtil) { }

        public async void CustomSignout()
        {
            //FormsAuthentication.SignOut();
            if (memberSignInManager != null)
            {
                await memberSignInManager.SignOutAsync();
            }
        }

        public bool UserIsAuthenticated() 
        {
            var loggedInUser = memberManager?.IsLoggedIn() ?? false;
            return loggedInUser;
        }
        public string GetAuthenticationPage(string uniqueId)
        {
            if (queryUtil == null) {
                return "";
            }
            var authPage = queryUtil.GetPageByUniqueId(uniqueId, ConfigurationModel.WebsiteContentTypes)?.Url() ?? string.Empty;
            return authPage;
        }

    }
}
