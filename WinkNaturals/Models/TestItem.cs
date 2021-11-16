using System;
using WinkNatural.Web.Common.Utils.Enum;

namespace WinkNaturals.Models
{
    public class TestItem
    {
        public int ItemID { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public decimal Weight { get; set; }
        public int ItemTypeID { get; set; }

        public string TinyImageUrl { get; set; }
        public string SmallImageUrl { get; set; }
        public string LargeImageUrl { get; set; }

        public string ShortDetail1 { get; set; }
        public string ShortDetail2 { get; set; }
        public string ShortDetail3 { get; set; }
        public string ShortDetail4 { get; set; }
        public string LongDetail1 { get; set; }
        public string LongDetail2 { get; set; }
        public string LongDetail3 { get; set; }
        public string LongDetail4 { get; set; }

        public bool IsVirtual { get; set; }
        public bool AllowOnAutoOrder { get; set; }

        public bool IsGroupMaster { get; set; }
        public string GroupMasterItemDescription { get; set; }
        public string GroupMembersDescription { get; set; }
        // public List<int> GroupMembers { get; set; }


        public bool IsDynamicKitMaster { get; set; }
        // public List<int> DynamicKitCategories { get; set; }
        // public Item StaticKitChildren { get; set; }

        public int PriceTypeID { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Price { get; set; }
        public decimal BV { get; set; }
        public decimal CV { get; set; }
        public Nullable<decimal> PriceEachOverride { get; set; }
        public Nullable<decimal> TaxableEachOverride { get; set; }
        public Nullable<decimal> CommissionableVolumeEachOverride { get; set; }
        public Nullable<decimal> BusinessVolumeEachOverride { get; set; }
        public Nullable<decimal> ShippingPriceEachOverride { get; set; }
        public decimal OtherPrice1 { get; set; }
        public decimal OtherPrice2 { get; set; }
        public decimal OtherPrice3 { get; set; }
        public decimal OtherPrice4 { get; set; }
        public decimal OtherPrice5 { get; set; }
        public decimal OtherPrice6 { get; set; }
        public decimal OtherPrice7 { get; set; }
        public decimal OtherPrice8 { get; set; }
        public decimal OtherPrice9 { get; set; }
        public decimal OtherPrice10 { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public string Field6 { get; set; }
        public string Field7 { get; set; }
        public string Field8 { get; set; }
        public string Field9 { get; set; }
        public string Field10 { get; set; }



        public bool OtherCheck1 { get; set; }
        public bool OtherCheck2 { get; set; }
        public bool OtherCheck3 { get; set; }
        public bool OtherCheck4 { get; set; }
        public bool OtherCheck5 { get; set; }

        public int SortOrder { get; set; }

        // IShoppingCartItem Members
        public Guid ID { get; set; }
        public decimal Quantity { get; set; }
        public string ParentItemCode { get; set; }
        public string GroupMasterItemCode { get; set; }
        public string DynamicKitCategory { get; set; }
        public ShoppingCartItemType Type { get; set; }

        public int CategoryID { get; set; }
        public int TotalItems { get; set; }

        public int InventoryFlag { get; set; }
        public string InventoryFlagDesc { get; set; }
        public ItemInventory ItemInventory
        {
            get
            {
                if (string.IsNullOrEmpty(Field1))
                    return ItemInventory.AVAILABLE;
                return (ItemInventory)Convert.ToInt32(Field1);
            }
            set
            {
                Field1 = ((int)value).ToString();
            }
        }

        public string Image_01
        {
            get
            {
                return "https://winkcloudfront.s3-us-west-1.amazonaws.com/content/images/products" + "/01-" + this.ItemCode + ".jpg";
            }
        }

        public string Image_02
        {
            get
            {
                return "https://winkcloudfront.s3-us-west-1.amazonaws.com/content/images/products" + "/02-" + this.ItemCode + ".jpg";
            }
        }

        public string Image_03
        {
            get
            {
                return "https://winkcloudfront.s3-us-west-1.amazonaws.com/content/images/products" + "/03-" + this.ItemCode + ".jpg";
            }
        }


    }
}
