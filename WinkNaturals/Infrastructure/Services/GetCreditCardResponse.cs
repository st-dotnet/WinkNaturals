using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.ExigoService;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Infrastructure.Services
{
    public class GetCreditCardResponse
    {
        public Address BillingAddress { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public int AutoOrderIDs { get; set; }
        public CreditCardType Type { get; set; }
        public string Token { get; set; }
        public string NameOnCard { get; set; }
     
        public int CustomerID { get; set; }
    }
}
