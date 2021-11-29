using Exigo.Api.Client;
using System.Collections.Generic;
using WinkNaturals.Models.Shopping;

namespace WinkNatural.Web.Services.DTO.Shopping
{
    public class TransactionalRequestModel
    {
        //public List<Item> ItemList { get; set; }
        public CreateOrderRequest CreateOrderRequest { get; set; }
        public ChargeCreditCardTokenRequest ChargeCreditCardTokenRequest { get; set; }
        public CreateAutoOrderRequest CreateAutoOrderRequest { get; set; }
        public SetAccountCreditCardTokenRequest SetAccountCreditCardTokenRequest { get; set; }
        public List<SetListItemRequest> SetListItemRequest { get; set; }
    }
}
