using Exigo.Api.Client;
using System;
using WinkNaturals.Infrastructure.Services.ExigoService;
using WinkNaturals.Models.Shopping.Checkout.Coupon;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;


namespace WinkNaturals.Models.Shopping.Checkout
{
    public class ShoppingCartCheckoutPropertyBag : BasePropertyBag
    {
        private string version = "1.0.0";
        private int expires = 31;

        #region Constructors
        public ShoppingCartCheckoutPropertyBag()
        {
            //Need to Develop.
            //  CustomerID = (Identity.Customer != null) ? Identity.Customer.CustomerID : 0;
            CustomerID = 1;
            Expires = expires;
            ShippingAddress = new ShippingAddress();
            ContainsSpecial = false;
        }
        #endregion

        #region Shared Properties
        public int CustomerID { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public int ShipMethodID { get; set; }
        public WN_Coupon Coupon { get; set; }
        public IPaymentMethod PaymentMethod { get; set; }
        public bool IsSubmitting { get; set; }
        public int NewOrderID { get; set; }
        public string OrderException { get; set; }
        public string PickedConsultant { get; set; }
        public string SelectedDistributor { get; set; }
        public ShippingAddress WillCallShippingAddress { get; set; }
        public bool UsePointsAsPayment { get; set; }
        public decimal QuantityOfPointsToUse { get; set; }
        public bool ContainsSpecial { get; set; }
        // for Paypal
        public string Nonce { get; set; }
        #endregion

        #region Shopping With Auto Order Only Properties
        public int NewAutoOrderID { get; set; }
        public FrequencyType AutoOrderFrequencyType { get; set; }
        public DateTime AutoOrderStartDate { get; set; }
        public ShippingAddress AutoOrderShippingAddress { get; set; }
        public ShippingAddress AutoOrderBillingAddress { get; set; }
        public IPaymentMethod AutoOrderPaymentMethod { get; set; }
        public bool AutoOrderBillingSameAsShipping { get; set; }
        public int AutoOrderShipMethodID { get; set; }
        #endregion

        #region Methods
        public override T OnBeforeUpdate<T>(T propertyBag)
        {
            propertyBag.Version = version;

            return propertyBag;
        }
        public override bool IsValid()
        {
            // Need to develop
            //  var currentCustomerID = (Identity.Customer != null) ? Identity.Customer.CustomerID : 0;
            return this.Version == version && this.CustomerID == 1;
        }
        #endregion
    }
}