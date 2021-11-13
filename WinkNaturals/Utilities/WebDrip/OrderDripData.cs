using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models;
using WinkNaturals.WebDrip;

namespace WinkNaturals.Utilities.WebDrip
{
    public class ShippingAddress
    {
        public dynamic address_1 { get; set; }
        public dynamic address_2 { get; set; }
        public dynamic city { get; set; }
        public dynamic state { get; set; }
        public dynamic postal_code { get; set; }
        public dynamic country { get; set; }
        public dynamic phone { get; set; }
    }

    public class BillingAddress
    {
        public dynamic address_1 { get; set; }
        public dynamic address_2 { get; set; }
        public dynamic city { get; set; }
        public dynamic state { get; set; }
        public dynamic postal_code { get; set; }
        public dynamic country { get; set; }
    }

    public class OrderDripData : BaseDrip
    {
        public dynamic provider { get; set; }
        public dynamic person_id { get; set; }
        public dynamic email { get; set; }
        public dynamic phone { get; set; }
        public dynamic action { get; set; }
        public dynamic order_id { get; set; }
        public dynamic grand_total { get; set; }
        public dynamic total_discounts { get; set; }
        public dynamic total_taxes { get; set; }
        public dynamic total_shipping { get; set; }
        public List<Items> items { get; set; }
        public ShippingAddress shipping_address { get; set; }
        public BillingAddress billing_address { get; set; }
        public OrderDripData()
        {
            items = new List<Items>();
            billing_address = new BillingAddress();
            shipping_address = new ShippingAddress();
        }
    }
}