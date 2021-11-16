using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WinkNaturals.Setting;
using System.Configuration;
namespace WinkNaturals.Utilities.Common
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

        public static class Company
        {
            public static int CorporateCalendarAccountID = 1;
            public static string Name = "Wink Naturals";


            public static string Address1 = "564 W 700 S, Suite 205";
            public static string Address2 = "";
            public static string City = "Pleasant Grove";
            public static string State = "UT";
            public static string Zip = "84062";
            public static string Country = "US";

            public static string Phone = "888-799-5656";
            public static string Email = "hello@winknaturals.com";
            public static string Facebook = "http://www.facebook.com/";
            public static string Twitter = "http://twitter.com/";
            public static string YouTube = "http://youtube.com/";
            public static string Blog = "http://blogger.net/blog/";
            public static string Pinterest = "http://www.pinterest.com";
            public static string Instagram = "http://www.instagram.com";
            public static string DefaultCompanyMessage = "This is our company statement.";


            public static string BaseBackofficeUrl = System.Configuration.ConfigurationManager.AppSettings["Company.BaseBackofficeUrl"];
            public static string BaseReplicatedUrl = System.Configuration.ConfigurationManager.AppSettings["Company.BaseReplicatedUrl"];
        }

        public class SMTPConfiguration
        {
            public string Server { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool EnableSSL { get; set; }
        }

        public static class EncryptionKeys
        {
            public static string General = "SDCLKJYAFS654ASF321FP87K"; // 24 characters 
            public static string MobileAppSilentLogin = "RddpVmge9RaUNzUC";

            public static class SilentLogins
            {
                public static string Key = /*GlobalUtilities.Api.CompanyKey +*/ "silentlogin";
                public static string IV = "kjJ6F6sf84vfV432"; // Must be 16 characters long
            }
        }
       
        public static string Encrypt(object value)
        {
            return Encrypt(value, Settings.EncryptionKeys.General);
        }
        public static string Encrypt(object value, object key)
        {
            var toEncrypt = JsonConvert.SerializeObject(value);
            var toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            var hashmd5 = new MD5CryptoServiceProvider();
            var keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key.ToString()));
            hashmd5.Clear();

            var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = tdes.CreateEncryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();

            var result = Convert.ToBase64String(resultArray, 0, resultArray.Length);

            result = EncryptionReplacements.Aggregate(result, (current, item) => current.Replace(item.Key, item.Value));
            return HttpUtility.UrlEncode(result);
        }
        public static string FormatWith(this string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            return string.Format(format, args);
        }
        private static readonly Dictionary<string, string> EncryptionReplacements = new Dictionary<string, string>
        {
            {"+", "_"},
            {"/", "-"},
            {"=", "!"}
        };
        
    }
}