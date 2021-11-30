using Newtonsoft.Json;

namespace WinkNaturals.Utilities.Common.YotPo.Domain
{
    public class AuthRequest
    {
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
