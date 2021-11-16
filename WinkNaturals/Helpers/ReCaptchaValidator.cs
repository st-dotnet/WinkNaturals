using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using WinkNaturals.Models;

namespace WinkNaturals.Helpers
{
    public class ReCaptchaValidator
    {
        private const string baseAddress = "https://www.google.com";
        private const string secretKey = "6Le68LsZAAAAALF-Bkh8pva6vSdGzhuZxthb5lJb";
        public static ReCaptchaValidationResult IsValid(string captchaResponse)
        {
            if (string.IsNullOrWhiteSpace(captchaResponse))
            {
                return new ReCaptchaValidationResult { Success = false };
            }

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("secret", secretKey),
                new KeyValuePair<string, string>("response", captchaResponse)
            };
            var content = new FormUrlEncodedContent(values);

            HttpResponseMessage response = client.PostAsync("/recaptcha/api/siteverify", content).Result;
            string verificationResponse = response.Content.ReadAsStringAsync().Result;
            var verificationResult = JsonConvert.DeserializeObject<ReCaptchaValidationResult>(verificationResponse);
            return verificationResult;
        }
    }
}
