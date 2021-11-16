using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Utilities.Common.YotPo.Domain
{
    public class PurchaseResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("errors")]
        public List<ErrorDesc> Errors { get; set; }
    }
    public class ErrorDesc
    {
        [JsonProperty("order_num")]
        public int OrderNum { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}