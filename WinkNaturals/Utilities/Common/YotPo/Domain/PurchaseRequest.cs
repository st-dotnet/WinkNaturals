using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Utilities.Common.YotPo.Domain
{
    public class PurchaseRequest
    {
        [JsonProperty("validate_data")]
        public bool ValidateData { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("utoken")]
        public string Utoken { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("customer_name")]
        public string CustomerName { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("order_date")]
        public string OrderDate { get; set; }

        [JsonProperty("currency_iso")]
        public string CurrencyIso { get; set; }

        [JsonProperty("products")]
        public Dictionary<string, Product> Products { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    public class Product
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string ImageUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("Product_tags")]
        public string ProducTags { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}