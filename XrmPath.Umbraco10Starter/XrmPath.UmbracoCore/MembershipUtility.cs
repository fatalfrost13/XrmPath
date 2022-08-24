using Umbraco.Cms.Web.Common.Security;

namespace XrmPath.UmbracoCore.Utilities
{
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
