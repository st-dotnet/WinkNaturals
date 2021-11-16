using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Utilities.Common.YotPo.Domain
{
    public class AuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

    }

    public class Status
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("code ")]
        public int Code { get; set; }

        [JsonProperty("error_type ")]
        public string ErrorType { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }
}