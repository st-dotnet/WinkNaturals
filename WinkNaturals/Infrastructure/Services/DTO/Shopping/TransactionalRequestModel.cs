using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Common;

namespace WinkNatural.Web.Services.DTO.Shopping
{
    public class TransactionalRequestModel
    {
        public CreateCustomerRequest createCustomerRequest { get; set; }
        public CreateOrderRequest createOrderRequest { get; set; }

        public ChargeCreditCardTokenRequest chargeCreditCardTokenRequest { get; set; }

        public CreateAutoOrderRequest createAutoOrderRequest { get; set; }

        public SetAccountCreditCardTokenRequest setAccountCreditCardTokenRequest { get; set; }

    }
}
