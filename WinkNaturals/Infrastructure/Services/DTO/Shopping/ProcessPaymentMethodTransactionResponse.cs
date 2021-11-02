using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static WinkNatural.Web.Services.Services.PaymentService;

namespace WinkNatural.Web.Services.DTO.Shopping
{
    public class ProcessPaymentMethodTransactionResponse
    {
         public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public int CVV { get; set; }
        public string ZipCode { get; set; }
        public string FullName { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        
        public string PaymentMethodId { get; set; }

        public int CustomerId { get; set; }
        public string CardNumber { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string EmailAddress { get; set; }
        public string ExternalId1 { get; set; }
        public string ExternalId2 { get; set; }



    }
}
