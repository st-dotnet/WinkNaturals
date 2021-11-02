using Microsoft.AspNetCore.Mvc;
namespace WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces
{
  //  [ModelBinder(typeof(IPaymentMethodModelBinder))]
   
    public interface IPaymentMethod
    {
        bool IsComplete { get; }
        bool IsValid { get; }
    }
}


