using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IAutoOrders
    {
        public object GetAutoOrderPaymentType(IPaymentMethod paymentMethod);




    }
}
