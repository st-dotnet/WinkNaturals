using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces
{
    public interface IPointAccount
    {
        int PointAccountID { get; set; }
        string PointAccountDescription { get; set; }
    }
}
