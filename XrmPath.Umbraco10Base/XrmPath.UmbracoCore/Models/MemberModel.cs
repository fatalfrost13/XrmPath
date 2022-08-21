using System;
using XrmPath.Helpers.Model;

namespace XrmPath.UmbracoCore.Models
{
    public class MemberModel
    {
        public bool UserValidated { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Approved { get; set; }
        public bool LockedOut { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public EmailStatus EmailStatus { get; set; } = new EmailStatus();
        public bool AccountVerified { get; set; }
        public bool AccountCreated { get; set; }

        public int Id { get; set; }
        public Guid? ContactId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string CompanyName {get; set; } = "";
        public bool RememberMe { get; set; }
        public string Password { get; set; } = "";
        public string Username { get; set; } = "";
        public DateTime LastLoginDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public DateTime LastPasswordChangeDate { get; set; }
        public int FailedPasswordAttempts { get; set; }
        public DateTime Created { get; set; }
        public string VerificationKey { get; set; } = "";
        public InputParameters InputParameters { get; set; } = new InputParameters();
        public int LanguageId { get; set; }
        public bool ContactConverted { get; set; }
        public bool AccountExists {get;set; } = false;
    }

    public class InputParameters
    {
        public int TemplateId { get; set; } = 0;
        public int RedirectId { get; set; } = 0;
        public string OldPassword { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public bool IsContact { get; set; } = false;
        public string EventId {get;set; } = string.Empty;
    }
}