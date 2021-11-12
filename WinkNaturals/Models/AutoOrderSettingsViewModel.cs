using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Checkout;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Models
{
    public class AutoOrderSettingsViewModel: IShoppingViewModel
    {
        public DateTime AutoOrderStartDate { get; set; }
        public FrequencyType AutoOrderFrequencyType { get; set; }

        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public string[] Errors { get; set; }
    }
}
