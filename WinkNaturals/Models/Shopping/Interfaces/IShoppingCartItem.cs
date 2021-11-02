using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IShoppingCartItem
    {
        Guid ID { get; set; }
        string ItemCode { get; set; }
        decimal Quantity { get; set; }
        string ParentItemCode { get; set; }
        string GroupMasterItemCode { get; set; }
        string DynamicKitCategory { get; set; }
        ShoppingCartItemType Type { get; set; }
        string Field4 { get; set; }
        string Field5 { get; set; }
        bool OtherCheck2 { get; set; }
        Nullable<decimal> PriceEachOverride { get; set; }
        Nullable<decimal> TaxableEachOverride { get; set; }
        Nullable<decimal> BusinessVolumeEachOverride { get; set; }
        Nullable<decimal> CommissionableVolumeEachOverride { get; set; }
        Nullable<decimal> ShippingPriceEachOverride { get; set; }



    }
}
