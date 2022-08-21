using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using XrmPath.Web.Helpers;
using XrmPath.Web.Helpers.Utils;
using XrmPath.Web.Models;

namespace XrmPath.Web.Controllers
{
    public class MembershipController : ApiController
    {
        // GET: Membership
        public HttpResponseMessage MemberLoginPost(MemberModel model)
        {
            var authenticationModel = MembershipHelper.MemberLogin(model);
            var responseMessage = Request.CreateResponse(HttpStatusCode.Accepted, authenticationModel);
            return responseMessage;
        }
        public HttpResponseMessage MemberLogoutPost()
        {
            MembershipUtility.CustomSignout();
            var defaultLoginPage = MembershipHelper.GetAuthenticationPage(UmbracoCustomLookups.LoginPage);
            var authenticationModel = new MemberModel { RedirectUrl = defaultLoginPage };
            var responseMessage = Request.CreateResponse(HttpStatusCode.Accepted, authenticationModel);
            return responseMessage;
        }

        public HttpResponseMessage ForgotPasswordPost(MemberModel model)
        {
            var authenticationModel = new MemberModel
            {
                EmailStatus = MembershipHelper.SendForgotPassword(model.Username, model.InputParameters.TemplateId)
            };
            var responseMessage = Request.CreateResponse(HttpStatusCode.Accepted, authenticationModel);
            return responseMessage;
        }

        public HttpResponseMessage ChangePasswordPost(MemberModel model)
        {
            var authenticationModel = MembershipHelper.ChangePassword(model);
            var responseMessage = Request.CreateResponse(HttpStatusCode.Accepted, authenticationModel);
            return responseMessage;
        }

        public List<string> GetMembershipGroupList()
        {
            return MembershipHelper.GetMembershipGroupList();
        }

        public HttpResponseMessage PostSignUp(MemberModel model)
        {
            var authenticationModel = MembershipHelper.SignUp(model);
            var responseMessage = Request.CreateResponse(HttpStatusCode.Accepted, authenticationModel);
            return responseMessage;
        }
    }
}