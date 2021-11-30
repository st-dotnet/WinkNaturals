using Exigo.Api.Client;
using System;
using WinkNaturals.Infrastructure.Services.ExigoService.BankAccount;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.Interfaces;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Models.Shopping
{
    public class AutoOrders : IAutoOrders
    {
        private readonly IExigoApiContext _exigoApiContext;
        private readonly ICustomerAutoOreder _customerAutoOreder;
        public AutoOrders(IExigoApiContext exigoApiContext, ICustomerAutoOreder customerAutoOreder)
        {
            _exigoApiContext = exigoApiContext;
            _customerAutoOreder = customerAutoOreder;
        }
        public object GetAutoOrderPaymentType(IPaymentMethod paymentMethod)
        {
            if (!(paymentMethod is IAutoOrderPaymentMethod)) throw new Exception("The provided payment method does not implement IAutoOrderPaymentMethod.");

            if (paymentMethod is CreditCard) return ((CreditCard)paymentMethod).AutoOrderPaymentType;
            if (paymentMethod is BankAccount) return ((BankAccount)paymentMethod).AutoOrderPaymentType;

            return AutoOrderPaymentType.WillSendPayment;
        }
    }
}

