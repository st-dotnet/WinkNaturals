using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;

namespace WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder
{
    public class AutoOrder : IAutoOrder
    {
        public AutoOrder()
        {
            ShippingAddress = new ShippingAddress();
            Details = new List<AutoOrderDetail>();
        }

        public int AutoOrderID { get; set; }
        public int CustomerID { get; set; }

        public string Description { get; set; }
        public int AutoOrderStatusID { get; set; }
        public int FrequencyTypeID { get; set; }
        public string CurrencyCode { get; set; }
        public int WarehouseID { get; set; }
        public int ShipMethodID { get; set; }
        public int AutoOrderPaymentTypeID { get; set; }
        public int AutoOrderProcessTypeID { get; set; }
        public string Notes { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? LastRunDate { get; set; }
        public DateTime? NextRunDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        public ShippingAddress ShippingAddress { get; set; }

        public IPaymentMethod PaymentMethod { get; set; }

        public List<AutoOrderDetail> Details { get; set; }

        public decimal Total { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal ShippingTotal { get; set; }
        public decimal DiscountTotal { get; set; }

        public decimal BVTotal { get; set; }
        public string FormattedBVTotal { get { return string.Format("{0:N0} {1}", BVTotal, "BV"); } }

        public decimal CVTotal { get; set; }
        public string FormattedCVTotal { get { return string.Format("{0:N0} {1}", CVTotal, "CV"); } }

        public string Other11 { get; set; }
        public string Other12 { get; set; }
        public string Other13 { get; set; }
        public string Other14 { get; set; }
        public string Other15 { get; set; }
        public string Other16 { get; set; }
        public string Other17 { get; set; }
        public string Other18 { get; set; }
        public string Other19 { get; set; }

        public static explicit operator AutoOrder(AutoOrderResponse v)
        {
            throw new NotImplementedException();
        }

        public string Other20 { get; set; }



        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }



        public string GetDescription()
        {
            if (String.IsNullOrEmpty(Description)) return this.Description;
            else
            {
                if (IsVirtualAutoOrder) return FrequencyTypeDescription + " Subscription Renewal";
                else return FrequencyTypeDescription + " Auto-ship";
            }
        }
        public string FrequencyTypeDescription
        {
            get
            {
                switch (FrequencyTypeID)
                {
                    default: return "Unknown";
                    case 1: return "Weekly";
                    case 2: return "Bi-Weekly";
                    case 3: return "Monthly";
                    case 4: return "Quarterly";
                    case 5: return "Bi-Yearly";
                    case 6: return "Yearly";
                    case 7: return "Bi-Monthly";
                    case 8: return "4-Week";
                    case 9: return "6-Week";
                }
            }
        }
        public bool IsActive
        {
            get { return AutoOrderStatusID == 0; }
        }
        public bool IsCancelled
        {
            get { return !IsActive; }
        }
        public bool IsVirtualAutoOrder
        {
            get { return Details != null && Details.All(d => d.IsVirtual); }
        }
        public bool IsBackupAutoOrder
        {
            get { return AutoOrderProcessTypeID == 2; }
        }
        public bool HasStarted
        {
            get { return StartDate <= DateTime.Now; }
        }
        public bool HasProcessedBefore
        {
            get { return LastRunDate != null; }
        }
        public bool WillBeStopped
        {
            get { return StopDate != null; }
        }
        public bool HasStopped
        {
            get { return WillBeStopped && ((DateTime)StopDate) < DateTime.Now; }
        }
        public bool HasValidPaymentMethod
        {
            get { return PaymentMethod != null && PaymentMethod.IsValid; }
        }
        public bool HasValidShippingAddress
        {
            get { return ShippingAddress.IsComplete; }
        }
    }
}