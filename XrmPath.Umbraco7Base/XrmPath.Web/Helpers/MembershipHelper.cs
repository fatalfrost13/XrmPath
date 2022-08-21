using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using XrmPath.Helpers.Utilities;
using XrmPath.Web.Email;
using XrmPath.Web.Helpers.UmbracoHelpers;
using XrmPath.Web.Helpers.Utils;
using XrmPath.Web.Models;
using Umbraco.Core.Models;

namespace XrmPath.Web.Helpers
{
    public static class MembershipHelper
    {
        public static MemberModel MemberLogin(MemberModel model)
        {
            //var authenticationModel = new AuthenticationModel();
            var userName = model.Username ?? string.Empty;
            var password = model.Password ?? string.Empty;

            var umbracoMember = ServiceUtility.MemberService.GetByUsername(userName);
            var membershipModel = ServiceUtility.MemberService.GetByUsername(userName)?.GetMember();

            if ((membershipModel != null && membershipModel.Approved && !membershipModel.LockedOut) && Membership.ValidateUser(userName, password))
            {
                //save last login
                membershipModel.Message = string.Empty;
                membershipModel.LastLoginDate = DateTime.UtcNow;
                membershipModel.UserValidated = true;
                LoginUser(membershipModel.Username, membershipModel.RememberMe, umbracoMember);

                if (!string.IsNullOrEmpty(model.InputParameters.RedirectUrl))
                {
                    membershipModel.RedirectUrl = model.InputParameters.RedirectUrl;
                }
            }
            else if (membershipModel != null)
            {
                //invalid user. find out why
                membershipModel.Message = string.Empty;
                if (!membershipModel.Approved)
                {
                    membershipModel.UserValidated = false;
                    membershipModel.Message = "Your account has not yet been approved.<br />Please check your email and follow the instructions to verify your account.";
                }
                else if (membershipModel.LockedOut)
                {
                    membershipModel.UserValidated = false;
                    membershipModel.Message = "Your account is currently locked. To unlock your account, please reset and change your password.";
                }
                else
                {
                    membershipModel.UserValidated = false;
                    membershipModel.Message = "Invalid login credentials.";
                }
            }

            if (membershipModel == null)
            {
                membershipModel = new MemberModel
                {
                    Message = "Account does not exist. Please sign up."
                };
            }

            return membershipModel;
        }

        public static MemberModel GetMember(this IMember member)
        {
            if (member != null)
            {
                var memberModel = new MemberModel
                {
                    Username = member.Username,
                    Email = member.Email,
                    Name = member.Name,
                    Id = member.Id,
                    FailedPasswordAttempts = member.FailedPasswordAttempts,
                    Approved = member.IsApproved,
                    LockedOut = member.IsLockedOut,
                    LastLoginDate = member.LastLoginDate,
                    LastLockoutDate = member.LastLockoutDate,
                    LastPasswordChangeDate = member.LastPasswordChangeDate,
                    Created = member.CreateDate
            };

                //var contactId = member.GetMemberValue(UmbracoCustomFields.ContactId);
                //if (!string.IsNullOrEmpty(contactId))
                //{
                //    Guid contactGuid;
                //    var isGuid = Guid.TryParse(contactId, out contactGuid);
                //    if (isGuid)
                //    {
                //        memberModel.ContactId = contactGuid;
                //    }
                //}

                var verificationKey = member.GetMemberValue(UmbracoCustomFields.VerificationKey);
                memberModel.VerificationKey = verificationKey;
                memberModel.LanguageId = member.GetMemberValueInt(UmbracoCustomFields.LanguageId);
                //memberModel.ContactConverted = member.GetMemberValueBoolean(UmbracoCustomFields.ContactConverted);
                return memberModel;
            }
            return null;
        }

        public static MemberModel GetMember(string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                var umbracoMember = ServiceUtility.MemberService.GetByUsername(userName);
                return umbracoMember.GetMember();
            }

            return null;
        }

        public static void LoginUser(string userName, bool persistent, IMember member = null)
        {
            FormsAuthentication.SetAuthCookie(userName, persistent);
            
        }

        public static EmailStatus SendForgotPassword(string email, int templateId = 0)
        {
            var notificationTemplateId = templateId;
            if (notificationTemplateId == 0)
            {
                notificationTemplateId = QueryUtility.GetPageByUniqueId(UmbracoCustomLookups.DefaultEmailTemplate, UmbracoCustomTypes.EmailTemplate).Id;
                //notificationTemplateId = int.Parse(ConfigurationManager.AppSettings["DefaultEmailTemplateId"]);
            }
            var emailStatus = new EmailStatus();

            if (notificationTemplateId > 0)
            {
                //update password to random guid before sending.

                //var membershipModel = GetWebMembershipContact(email);
                var membershipModel = ServiceUtility.MemberService.GetByEmail(email)?.GetMember();

                if (membershipModel != null)
                {
                    membershipModel.Password = ResetPassword(membershipModel);  //don't pass in a password to auto generate a random password.

                    var notificationNode = ServiceUtility.UmbracoHelper.GetById(notificationTemplateId);
                    var emailSubject = notificationNode.GetContentValue(UmbracoCustomFields.EmailSubject);
                    var emailTo = !string.IsNullOrEmpty(email) ? email : notificationNode.GetContentValue(UmbracoCustomFields.EmailAdmin);
                    var emailFrom = notificationNode.GetContentValue(UmbracoCustomFields.EmailFrom);
                    var emailMessage = notificationNode.GetContentValue(UmbracoCustomFields.EmailMessage);
                    var emailBcc = notificationNode.GetContentValue(UmbracoCustomFields.EmailBcc);
                    var emailTemplate = "/Email/StandardTemplate.html";

                    var loginPage = GetAuthenticationPage(UmbracoCustomLookups.LoginPage);
                    var changePasswordPage = GetAuthenticationPage(UmbracoCustomLookups.ChangePasswordPage);
                    //var loginUrl = $"{ApplicationCacheHelper.CurrentDomainUrl}{loginPage}?RedirectUrl={changePasswordPage}";
                    var loginUrl = SiteUrlUtility.GetFullUrl($"{loginPage}?RedirectUrl={changePasswordPage}");
                    var loginLink = $"<a href=\"{loginUrl}\">here</a>";

                    if (emailTo != string.Empty)
                    {
                        var replacements = DictionaryReplacement.RecoverPassword(membershipModel, loginLink);
                        emailStatus = EmailUtility.SendEmail(emailTo, emailFrom, emailBcc, emailSubject, emailMessage, emailTemplate, replacements);
                    }
                }
                else
                {
                    emailStatus.EmailSent = false;
                    emailStatus.ErrorMessage = "Your email could not be found in our system. Please create an account.";
                }
            }
            return emailStatus;
        }

       


        public static MemberModel ChangePassword(MemberModel model)
        {
            var membershipModel = Membership.GetUser(model.Username);
            try
            {
                var oldPasswordCorrect = Membership.ValidateUser(model.Username, model.InputParameters.OldPassword);
                if (!oldPasswordCorrect)
                {
                    model.Message = "The 'Old Password' you entered is incorrect. Please try again.";
                    return model;
                }

                var passwordChanged = membershipModel?.ChangePassword(model.InputParameters.OldPassword, model.Password) ?? false;
                if (!passwordChanged)
                {
                    model.Message = "The 'Old Password' you entered is incorrect. Please try again.";
                }
            }
            catch (Exception ex)
            {
                //model.Message = $"The '{ex}'";
                model.Message = $"The 'New Password' you entered must have a minimum of 8 characters. {ex}";
            }

            return model;
        }

        private static string ResetPassword(MemberModel model)
        {
            try
            {
                var membershipModel = Membership.GetUser(model.Username);
                if (membershipModel != null)
                {
                    var member = ServiceUtility.MemberService.GetByUsername(model.Email);

                    //check if user is locked out:
                    if (membershipModel.IsLockedOut)
                    {
                        if (member != null && member.IsLockedOut)
                        {
                            member.IsLockedOut = false;
                            ServiceUtility.MemberService.Save(member);
                        }
                    }

                    var newPassword = StringUtility.RandomAlphaNumericString(10);
                    ServiceUtility.MemberService.SavePassword(member, newPassword);
                    //var newPassword = membershipModel.ResetPassword();
                    return newPassword;
                }
            }
            catch (Exception ex)
            {
                model.Message = $"The 'New Password' you entered must have a minimum of 8 characters. Error: {ex}";
                return null;
            }

            return null;
        }

        public static List<string> GetMembershipGroupList()
        {
            var allRoles = ServiceUtility.MemberService.GetAllRoles().ToList();
            var defaultRole = allRoles.SingleOrDefault(i => i == UmbracoCustomLookups.WebsiteUser);
            var rolesOrdered = new List<string>();
            
            if (defaultRole != null)
            {
                //add the default role first
                rolesOrdered.Add(defaultRole);
                allRoles = allRoles.Where(i => i != defaultRole).OrderBy(i => i).ToList();
            }

            if (allRoles.Any())
            {
                rolesOrdered.AddRange(allRoles);
            }

            return rolesOrdered;
        }

        public static MemberModel VerifyAccount(string email, string verificationKey)
        {
            var memberModel = new MemberModel();

            //Guid webMemberId;
            //var validId = Guid.TryParse(verificationKey, out webMemberId);
            //var member = validId ? DataContext.appl_webmembershipSet.SingleOrDefault(i => i.appl_ApplicationName == MembershipUtility.WebMembershipApplicationName && i.appl_webmembershipId == webMemberId && i.statecode == appl_webmembershipState.Active) : null;
            //var member = validId ? MemberService.GetMembersByPropertyValue(UmbracoCustomFields.VerificationKey, verificationKey).SingleOrDefault() : null;
            var member = ServiceUtility.MemberService.GetByUsername(email);

            if (member != null)
            {
                //verifyAccount.WebMembershipId = memberId;
                memberModel = member.GetMember();
                memberModel.AccountVerified = true;
                if (!memberModel.Approved)
                {
                    if (memberModel.VerificationKey == verificationKey)
                    {
                        member.IsApproved = true;
                        ServiceUtility.MemberService.Save(member);
                        memberModel.Message = "Your account has been verified.";
                    }
                    else
                    {
                        memberModel.AccountVerified = false;
                        memberModel.Message = "Your account verification key is incorrect.";
                    }

                }
                else
                {
                    memberModel.Message = "Your account has already been verified";
                }
            }
            else
            {
                memberModel.AccountVerified = false;
                memberModel.Message = "Invalid Request";
            }
            return memberModel;
        }

        public static string GetAuthenticationPage(string uniqueId)
        {
            var authPage = QueryUtility.GetPageByUniqueId(uniqueId, ConfigurationModel.WebsiteContentTypes)?.NodeUrl() ?? string.Empty;
            return authPage;
        }

        public static bool UserIsAuthenticated()
        {
            var isAuthenticated = HttpContext.Current?.User?.Identity?.IsAuthenticated != null && HttpContext.Current.User.Identity.IsAuthenticated;
            return isAuthenticated;
        }
        public static MemberModel SignUp(MemberModel model)
        {

            var umbracoMember = ServiceUtility.MemberService.GetByUsername(model.Email);
            if (umbracoMember == null)
            {
                //does not exist in umbraco, create umbraco account
                CreateUmbracoAccount(model, false, true);
            }
            else
            {
                model.AccountExists = true;
                model.EmailStatus.EmailSent = false;
            }

            if (model.AccountExists)
            {
                model.Message = "An account with this email already exists.";
            }

            return model;
        }

        public static EmailStatus CreateUmbracoAccount(MemberModel contactModel, bool autoApprove = true,  bool setPassword = false)
        {
            var emailStatus = new EmailStatus();


            var membershipModel = Membership.GetUser(contactModel.Email);
            if (membershipModel != null)
            {
                emailStatus = new EmailStatus { EmailSent = false, ErrorMessage = "Umbraco user account already exists." };
                return emailStatus;
            }

            var memberType = ServiceUtility.MemberService.GetDefaultMemberType();
            var name = $"{contactModel.FirstName} {contactModel.LastName}";
            var umbracoMember = ServiceUtility.MemberService.CreateMember(contactModel.Email, contactModel.Email, name, memberType);
            umbracoMember.SetValue(UmbracoCustomFields.VerificationKey, Guid.NewGuid().ToString());
            //umbracoMember.SetValue(UmbracoCustomFields.ContactId, contactModel.Id.ToString());
            umbracoMember.SetValue(UmbracoCustomFields.LanguageId, contactModel.LanguageId.ToString());
            umbracoMember.IsApproved = autoApprove;
            ServiceUtility.MemberService.Save(umbracoMember);

            if (membershipModel == null)
            {
                membershipModel = Membership.GetUser(contactModel.Email);
            }
            if (string.IsNullOrEmpty(contactModel.Password) || setPassword)
            {
                if (membershipModel != null)
                {
                    //var newPassword = Guid.NewGuid().ToString();
                    var password = contactModel.Password;
                    if (string.IsNullOrEmpty(password))
                    {
                        var newPassword = StringUtility.RandomAlphaNumericString(10);
                        password = newPassword;
                    }

                    ServiceUtility.MemberService.SavePassword(umbracoMember, password);
                    //var newPassword = membershipModel.ResetPassword();
                    contactModel.Password = password;
                }
            }

            //var emailStatus = autoApprove ? SendProfileLink(contactModel, contactModel.Password) : SendVerifyAccount(contactModel);

            
                if (!autoApprove)
                {
                    //send verify email to allow member to activate account.
                    emailStatus = SendVerifyAccount(contactModel, 0);
                }
                //assign member to default group
                ServiceUtility.MemberService.AssignRole(umbracoMember.Username, UmbracoCustomLookups.WebsiteUser);
            

            return emailStatus;
        }

        public static EmailStatus SendVerifyAccount(MemberModel contactModel, int templateId = 0)
        {
            var emailStatus = new EmailStatus();
            var email = contactModel.Email;
            var umbracoMember = ServiceUtility.MemberService.GetByUsername(email);

            if (umbracoMember == null)
            {
                emailStatus.EmailSent = false;
                emailStatus.ErrorMessage = "Account verification not sent because email could not be found in the system.";
                return emailStatus;
            }

            var verificationPage = GetAuthenticationPage(UmbracoCustomLookups.VerifyAccountPage);
            var verificationKey = umbracoMember.GetMemberValue(UmbracoCustomFields.VerificationKey);
            //var verificationUrl = $"{ApplicationCacheHelper.CurrentDomainUrl}{verificationPage}?email={email}&verificationKey={verificationKey}&vue=0";
            var verificationUrl = SiteUrlUtility.GetFullUrl($"{verificationPage}?email={email}&verificationKey={verificationKey}&vue=0");
            var verificationLink = $"<a href=\"{verificationUrl}\">{verificationUrl}</a>";

            var notificationTemplateId = templateId;
            if (notificationTemplateId == 0)
            {
                //notificationTemplateId = int.Parse(ConfigurationManager.AppSettings["DefaultEmailTemplateId"]);
                notificationTemplateId = QueryUtility.GetPageByUniqueId(UmbracoCustomLookups.SignUpEmailTemplate, UmbracoCustomTypes.EmailTemplate).Id;
            }

            if (notificationTemplateId > 0)
            {
                var notificationNode = ServiceUtility.UmbracoHelper.GetById(notificationTemplateId);
                var emailSubject = notificationNode.GetContentValue(UmbracoCustomFields.EmailSubject);
                var emailTo = !string.IsNullOrEmpty(email) ? email : notificationNode.GetContentValue(UmbracoCustomFields.EmailAdmin);
                var emailFrom = notificationNode.GetContentValue(UmbracoCustomFields.EmailFrom);
                var emailMessage = notificationNode.GetContentValue(UmbracoCustomFields.EmailMessage);
                var emailBcc = notificationNode.GetContentValue(UmbracoCustomFields.EmailBcc);
                var emailTemplate = "/Email/StandardTemplate.html";

                if (emailTo != string.Empty)
                {
                    var replacements = DictionaryReplacement.Signup(contactModel, verificationLink);
                    emailStatus = EmailUtility.SendEmail(emailTo, emailFrom, emailBcc, emailSubject, emailMessage, emailTemplate, replacements);
                }
            }

            return emailStatus;
        }
    }
}