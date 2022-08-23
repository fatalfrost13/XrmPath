using Umbraco.Cms.Web.Common.Security;

namespace XrmPath.UmbracoCore.Utilities
{
    public class MembershipUtility
    {
        private IMemberSignInManager _memberSignInManager;
        public MembershipUtility(IMemberSignInManager memberSignInManager) {
            if (_memberSignInManager == null)
            {
                _memberSignInManager = memberSignInManager;
            }
        }
        public async void CustomSignout()
        {
            //FormsAuthentication.SignOut();
            await _memberSignInManager.SignOutAsync();
        }
    }
}
