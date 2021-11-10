using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using WinkNaturals.Models.Shopping.PointAccount.Request;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IAutoOrders
    {
      public  object GetAutoOrderPaymentType(IPaymentMethod paymentMethod);

     

    
    }
}
