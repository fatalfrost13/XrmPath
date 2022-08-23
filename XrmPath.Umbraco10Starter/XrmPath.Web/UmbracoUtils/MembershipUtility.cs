using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;

namespace XrmPath.UmbracoUtils
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
