using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinkNaturals.Utilities.Common.Settings;

namespace WinkNatural.Web.Common.Utils
{
    public static class Settings
    {
        public static string BaseImageURL = "https://winkcloudfront.s3-us-west-1.amazonaws.com/content/images/products";
        public static class Emails
        {
            public static string NoReplyEmail = "hello@winknaturals.com";
            public static string ContactUsEmail = "hello@winknaturals.com";
            public static string VerifyEmailUrl = Company.BaseBackofficeUrl + "/verifyemail";

            // NEED NEW CREDS FROM CLIENT IF THEY ARE TO SEND ANY EMAILS FROM THE WEB
            public static class SMTPConfigurations
            {
                public static SMTPConfiguration Default = new SMTPConfiguration
                {
                    Server = "mail.exigo.com",
                    Port = 26,
                    Username = "noreply@exigonow.com",
                    Password = "whodaman",
                    EnableSSL = false
                };
            }
        }
      
    }
}
