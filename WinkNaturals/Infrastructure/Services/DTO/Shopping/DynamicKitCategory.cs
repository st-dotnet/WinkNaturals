using System.Collections.Generic;

namespace WinkNatural.Web.Services.DTO.Shopping
{
    public class DynamicKitCategory
    {
        public DynamicKitCategory()
        {
            this.Items = new List<DynamicKitCategoryItem>();
        }
        public int DynamicKitCategoryID { get; set; }
        public string DynamicKitCategoryDescription { get; set; }
        public decimal Quantity { get; set; }

        public List<DynamicKitCategoryItem> Items { get; set; }
    }
}
