using Exigo.Api.Client;
using System;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Utilities;

namespace WinkNaturals.Models
{
    public class ShoppingCartItem : IShoppingCartItem
    {
        public ShoppingCartItem()
        {
            ID = Guid.NewGuid();
            ItemCode = string.Empty;
            Quantity = 0;
            ParentItemCode = string.Empty;
            DynamicKitCategory = string.Empty;
            GroupMasterItemCode = string.Empty;
            Type = ShoppingCartItemType.Order;
            Field4 = "0";
            Field5 = string.Empty;
            PriceEachOverride = null;
        }
        public Guid ID
        {
            get
            {
                if (_id == null)
                {
                    _id = Guid.NewGuid();
                }
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        private Guid _id;
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string ParentItemCode { get; set; }
        public string GroupMasterItemCode { get; set; }
        public string DynamicKitCategory { get; set; }
        public ShoppingCartItemType Type { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public bool OtherCheck2 { get; set; }
        public decimal? PriceEachOverride { get; set; }
        public decimal? TaxableEachOverride { get; set; }
        public decimal? BusinessVolumeEachOverride { get; set; }
        public decimal? CommissionableVolumeEachOverride { get; set; }
        public decimal? ShippingPriceEachOverride { get; set; }


        public ShoppingCartItem(IShoppingCartItem item)
        {
            ID = (item.ID != Guid.Empty) ? item.ID : Guid.NewGuid();
            ItemCode = GlobalUtilities.Coalesce(item.ItemCode);
            Quantity = item.Quantity;
            ParentItemCode = GlobalUtilities.Coalesce(item.ParentItemCode);
            DynamicKitCategory = GlobalUtilities.Coalesce(item.DynamicKitCategory);
            GroupMasterItemCode = GlobalUtilities.Coalesce(item.GroupMasterItemCode);
            Type = item.Type;
            Field4 = item.Field4;
            Field5 = item.Field5;
            OtherCheck2 = item.OtherCheck2;
            PriceEachOverride = item.PriceEachOverride;
            TaxableEachOverride = item.PriceEachOverride;
            CommissionableVolumeEachOverride = item.PriceEachOverride;
            BusinessVolumeEachOverride = item.PriceEachOverride;
            ShippingPriceEachOverride = item.PriceEachOverride;
        }
        public bool IsDynamicKitMember
        {
            get
            {
                return (!string.IsNullOrEmpty(this.ParentItemCode));
            }
        }

        public static explicit operator OrderDetailRequest(ShoppingCartItem v)
        {
            throw new NotImplementedException();
        }
    }
}
