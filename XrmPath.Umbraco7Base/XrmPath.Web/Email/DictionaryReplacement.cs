using System;
using System.Collections.Specialized;
using XrmPath.Web.Models;

namespace XrmPath.Web.Email
{
    public static class DictionaryReplacement
    {
        public static ListDictionary RecoverPassword(MemberModel memberModel, string loginLink)
        {
            //var dateNow = DateTime.Now.ConvertFromCrmDateTime(service);
            var dateNow = DateTime.Now;
            var replacements = new ListDictionary
                    {
                        { "&lt;&lt;Date&gt;&gt;", dateNow.ToString("MMMM d, yyyy") },
                        { "&lt;&lt;DateTime&gt;&gt;", dateNow.ToString("MMMM d, yyyy h:mm tt") },
                        //{ "&lt;&lt;FirstName&gt;&gt;", contact.FirstName },
                        //{ "&lt;&lt;LastName&gt;&gt;", contact.LastName },
                        { "&lt;&lt;Name&gt;&gt;", memberModel.Name },
                        { "&lt;&lt;Email&gt;&gt;", memberModel.Username },
                        { "&lt;&lt;Password&gt;&gt;", memberModel.Password },
                        { "&lt;&lt;LoginLink&gt;&gt;", loginLink }
                    };
            return replacements;
        }

        public static ListDictionary Signup(MemberModel contactModel, string verificationLink)
        {

            var replacements = new ListDictionary
                    {
                        { "&lt;&lt;FirstName&gt;&gt;", contactModel.FirstName },
                        { "&lt;&lt;LastName&gt;&gt;", contactModel.LastName },
                        { "&lt;&lt;Password&gt;&gt;", contactModel.Password },
                        { "&lt;&lt;Email&gt;&gt;", contactModel.Email },
                        { "&lt;&lt;VerificationLink&gt;&gt;", verificationLink }
                    };
            return replacements;
        }
    }
}