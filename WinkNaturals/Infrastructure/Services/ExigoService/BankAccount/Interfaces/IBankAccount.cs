using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.Interfaces;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Infrastructure.Services.ExigoService.BankAccount.Interfaces
{
    public interface IBankAccount : IPaymentMethod, IAutoOrderPaymentMethod
    {
        BankAccountType Type { get; set; }

        string NameOnAccount { get; set; }
        string BankName { get; set; }
        string AccountNumber { get; set; }
        string RoutingNumber { get; set; }
        Address BillingAddress { get; set; }

        new int[] AutoOrderIDs { get; set; }
    }
}

