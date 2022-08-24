using Umbraco.Cms.Web.Common.Security;

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
    }
}
