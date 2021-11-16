using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;

namespace WinkNaturals.Models.Shopping.Interfaces
{
 
        public interface IAutoOrders
        {
            public object GetAutoOrderPaymentType(IPaymentMethod paymentMethod);
        }
    
}
