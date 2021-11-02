using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.DTO
{
    public class EnrollmentResponse
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

        public bool IsDynamicKitMaster { get; set; }
        public List<EnrollmentResponse> StaticKitChildren { get; set; }

        public int PriceTypeID { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Price { get; set; }
        public decimal BV { get; set; }
        public decimal CV { get; set; }
        public decimal? PriceEachOverride { get; set; }
        public decimal? TaxableEachOverride { get; set; }
        public decimal? CommissionableVolumeEachOverride { get; set; }
        public decimal? BusinessVolumeEachOverride { get; set; }
        public decimal? ShippingPriceEachOverride { get; set; }
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

        public int CategoryID { get; set; }
        public int TotalItems { get; set; }

        public int InventoryFlag { get; set; }
        public string InventoryFlagDesc { get; set; }

        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public FileContentResult ProductImage { get; set; } 
        public string Image_01
        {
            get
            {
                return Common.Utils.Settings.BaseImageURL + "/01-" + this.ItemCode + ".jpg";
            }
        }

        public string Image_02
        {
            get
            {
                return Common.Utils.Settings.BaseImageURL + "/02-" + this.ItemCode + ".jpg";
            }
        }

        public string Image_03
        {
            get
            {
                return Common.Utils.Settings.BaseImageURL + "/03-" + this.ItemCode + ".jpg";
            }
        }
    }
}
