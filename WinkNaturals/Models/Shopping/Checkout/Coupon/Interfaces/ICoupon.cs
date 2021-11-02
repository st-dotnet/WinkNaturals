using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces
{
   public  interface ICoupon
    {
        string Code { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        int MinSubtotal { get; set; }
        bool FreeShipping { get; set; }
        int PercentOff { get; set; }
        int ItemID { get; set; }
        string ItemCode { get; set; }
        int WebCategoryID { get; set; }
        int MinRank { get; set; }
        int MaxRank { get; set; }
        string MinRankDescription { get; set; }
        string MaxRankDescription { get; set; }
        int MaxUseLimit { get; set; }
        bool SingleUseByCustomer { get; set; }
        bool DisplayOnWeb { get; set; }
        DateTime CreatedDate { get; set; }
        Guid RowGuid { get; set; }
        string CouponCode { get; set; }
        decimal CouponQuantity { get; set; }
        decimal CouponPriceEach { get; set; }
        string CouponItemDescription { get; set; }
    }
}