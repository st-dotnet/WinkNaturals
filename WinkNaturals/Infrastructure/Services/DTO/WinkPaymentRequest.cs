using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Web.Services.DTO
{
    public class WinkPaymentRequest
    {
        public BankCardTransactionEnum cardType { get; set; }
        public string cardName { get; set; }
        public string CardNumber { get; set; }
        public string CardExpirationMonth { get; set; }
        public string CardExpirationYear { get; set; }
        public double Amount { get; set; }
    }
    public enum BankCardTransactionEnum
    {
        XS_BCT_TYPE_SALE
    }
}
