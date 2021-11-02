using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;

namespace WinkNaturals.Models.Shopping.Checkout.Coupon
{
    public class WN_Coupon : ICoupon
    {
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MinSubtotal { get; set; }
        public bool FreeShipping { get; set; }
        public int PercentOff { get; set; }
        public int ItemID { get; set; }
        public string ItemCode { get; set; }
        public int WebCategoryID { get; set; }
        public int MinRank { get; set; }
        public int MaxRank { get; set; }
        public string MinRankDescription { get; set; }
        public string MaxRankDescription { get; set; }
        public int MaxUseLimit { get; set; }
        public bool SingleUseByCustomer { get; set; }
        public bool DisplayOnWeb { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid RowGuid { get; set; }
        public string CouponCode { get; set; }
        public decimal CouponQuantity { get; set; }
        public decimal CouponPriceEach { get; set; }
        public string CouponItemDescription { get; set; }
        public string CustomerTypes { get; set; }
    }
}