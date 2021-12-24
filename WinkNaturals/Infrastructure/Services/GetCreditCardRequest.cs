using WinkNatural.Web.Services.DTO.Shopping;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Infrastructure.Services
{
    public class GetCreditCardRequest
    {

        public Address BillingAddress { get; set; }
        public int ExpirationMonth { get; set; }
        public int ExpirationYear { get; set; }
        public int AutoOrderIDs { get; set; }
        public CreditCardType Type  { get; set; }  
        public string Token { get; set; }
       public string NameOnCard { get; set; }

    }
}
