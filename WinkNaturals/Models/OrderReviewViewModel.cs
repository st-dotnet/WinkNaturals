using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Shopping.CalculateOrder;
using WinkNaturals.Models.Shopping.Checkout;
using WinkNaturals.Models.Shopping.Checkout.Coupon;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces.PointAccount;
using WinkNaturals.Models.Shopping.Orders;

namespace WinkNaturals.Models
{
    public class OrderReviewViewModel: IShoppingViewModel
    {
        public IEnumerable<IItem> Items { get; set; }
        public OrderCalculationResponse OrderTotals { get; set; }
        public IEnumerable<IShipMethod> ShipMethods { get; set; }
        public ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        public CustomerPointAccount LoyaltyPointAccount { get; set; }
        public bool HasValidPointAccount { get; set; }
        public decimal QuantityOfPointsToUse { get; set; }
        public WN_Coupon Coupon { get; set; }
        public string[] Errors { get; set; }

        // Auto Order Properties
        public OrderCalculationResponse AutoOrderTotals { get; set; }
        public IEnumerable<IShipMethod> AutoOrderShipMethods { get; set; }
    }
}
