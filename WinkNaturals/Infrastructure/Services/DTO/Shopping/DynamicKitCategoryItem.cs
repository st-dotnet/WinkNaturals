using System;
using WinkNatural.Web.Common.Utils.Enum;
using WinkNaturals.Models;

namespace WinkNatural.Web.Services.DTO.Shopping
{
    public class DynamicKitCategoryItem
    {
        public int ItemID { get; set; }
        public string ItemDescription { get; set; } 
        public string TinyImageUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string LargeImageUrl { get; set; } 
        // IShoppingCartItem Properties
        public Guid ID { get; set; }
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string ParentItemCode { get; set; }
        public int MasterItemID { get; set; }
        public string GroupMasterItemCode { get; set; }
        public string DynamicKitCategory { get; set; }
        public int DynamicKitCategoryID { get; set; }
        public ShoppingCartItemType Type { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public bool OtherCheck2 { get; set; }
        public decimal? PriceEachOverride { get; set; }
        public decimal? TaxableEachOverride { get; set; }
        public decimal? CommissionableVolumeEachOverride { get; set; }
        public decimal? BusinessVolumeEachOverride { get; set; }
        public decimal? ShippingPriceEachOverride { get; set; }
    }
}
