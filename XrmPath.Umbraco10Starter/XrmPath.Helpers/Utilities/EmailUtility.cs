using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using XrmPath.Helpers.Model;

namespace XrmPath.Helpers.Utilities
{
    public static class EmailUtility
    {
        public static EmailStatus SendEmail(string toEmail, string fromEmail, string bccEmail, string subject, string emailMessage, string emailTemplate, ListDictionary replacements)
        {
            var msg = GetMailMessage(toEmail, fromEmail, bccEmail, subject, emailMessage, emailTemplate, replacements);
            var emailSent = SendEmail(msg);
            return emailSent;
        }
        public static EmailStatus SendEmail(string toEmail, string fromEmail, string bccEmail, string subject, string emailMessage)
        {
            var msg = new MailMessage();
            try
            {

                //var md = new MailDefinition
                //{
                //    From = fromEmail,
                //    IsBodyHtml = true,
                //    Subject = subject
                //};
                //msg = md.CreateMailMessage(toEmail, null, emailMessage,new System.Web.UI.Control());
                msg = new MailMessage(fromEmail, toEmail, subject, emailMessage);
                msg.IsBodyHtml = true;
                if (!string.IsNullOrEmpty(bccEmail))
                {
                    var bccEmailList = bccEmail.StringToSet().Distinct();
                    foreach (var email in bccEmailList)
                    {
                        msg.Bcc.Add(email);
                    }
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, "XrmPath.Helpers caught error on EmailUtility.SendEmail()");
                Console.WriteLine(ex.Message);
            }
         
            var emailSent = SendEmail(msg);
            return emailSent;
        }
        public static EmailStatus SendEmail(MailMessage msg)
        {
            var emailSent = new EmailStatus { EmailSent = false, ErrorMessage = "" };

            try
            {
                var smtp = new SmtpClient();
                smtp.Send(msg);
                emailSent.EmailSent = true;
                emailSent.ErrorMessage = "";
            }
            catch (Exception ex)
            {
                emailSent.EmailSent = false;
                emailSent.ErrorMessage = ex.Message;
                var emailTo = string.Join(", ", msg.To.Select(i => i.Address));
                //Serilog.Log.Warning($"XrmPath.Helpers warning: Failure to send email on EmailUtility.SendEmail to: {emailTo}. Message: {ex}");
            }

            return emailSent;
        }

        public static MailMessage GetMailMessage(string toEmail, string fromEmail, string bccEmail, string subject, string emailMessage, string emailTemplate, ListDictionary replacements)
        {
            var msg = new MailMessage();
            try
            {
                string body;
                //var filePath = HttpContext.Current.Request.PhysicalApplicationPath;
                var filePath = HttpHelper.HttpContext?.Request.PathBase ?? "";
                using (var sr = new StreamReader(filePath + @emailTemplate))
                {
                    body = sr.ReadToEnd();
                }

                body = body.Replace("&lt;&lt;EmailMessage&gt;&gt;", emailMessage);
                body = FormatEmailBody(body);

                //var md = new MailDefinition
                //{
                //    From = fromEmail,
                //    IsBodyHtml = true,
                //    Subject = subject
                //};
                //msg = md.CreateMailMessage(toEmail, replacements, body, new System.Web.UI.Control());
                msg = new MailMessage(fromEmail, toEmail, subject, emailMessage);
                msg.IsBodyHtml = true;

                if (!string.IsNullOrEmpty(bccEmail))
                {
                    var bccEmailList = bccEmail.StringToSet().Distinct();
                    foreach (var email in bccEmailList)
                    {
                        msg.Bcc.Add(email);
                    }
                }
            }
            catch (Exception ex)
            {
                //Serilog.Log.Error(ex, "XrmPath.Helpers caught error on EmailUtility.GetMailMessage()");
                Console.WriteLine(ex.Message);
            }
            return msg;
        }

        /// <summary>
        /// Custom replacement body text. Need to convert relative links to absolute because relative links won't work in emails
        /// </summary>
        /// <returns></returns>
        public static string FormatEmailBody(string bodyText)
        {
            var body = bodyText;
            //rich text content may have links to media items. relative links won't work in email so try to prepend domain
            body = body.Replace("href=\"/", $"href=\"{WebUtility.GetDomain()}/");
            body = body.Replace("src=\"/", $"src=\"{WebUtility.GetDomain()}/");

            return body;
        }

        public static string ApplyReplacements(this string bodyText, ListDictionary replacements)
        {
            var replacementBodyText = bodyText;
            var replacementKeys = replacements.Keys;
            foreach (var replacementKey in replacementKeys)
            {
                var value = replacements[replacementKey]?.ToString() ?? string.Empty;
                if (value != null)
                {
                    var key = replacementKey.ToString() ?? "";
                    replacementBodyText = replacementBodyText.Replace(key, value);
                }
            }
            return replacementBodyText;
        }
    }
}