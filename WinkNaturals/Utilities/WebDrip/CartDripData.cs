using Newtonsoft.Json;
using System.Collections.Generic;
using WinkNaturals.Utilities.WebDrip;

namespace WinkNaturals.WebDrip
{
    public class Items
    {

        public dynamic product_id { get; set; }
        public dynamic sku { get; set; }
        public dynamic name { get; set; }
        public dynamic price { get; set; }
        public dynamic quantity { get; set; }
        public dynamic image_url { get; set; }

    }
    public class CartDripData : BaseDrip
    {
        public dynamic provider { get; set; }
        public dynamic person_id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public dynamic email { get; set; }
        public dynamic action { get; set; }
        public dynamic cart_url { get; set; }
        public dynamic cart_id { get; set; }
        public List<Items> items { get; set; }

        public CartDripData()
        {
            this.items = new List<Items>();
        }
    }
}
