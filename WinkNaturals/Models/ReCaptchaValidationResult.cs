using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models
{
    public class ReCaptchaValidationResult
    {
        public bool Success { get; set; }
        public string HostName { get; set; }
        [JsonProperty("challenge_ts")]
        public string TimeStamp { get; set; }
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}
