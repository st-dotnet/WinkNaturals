using System;
using System.Collections.Generic;

namespace WinkNatural.Web.Services.DTO.Shopping
{
    public class ItemCategoryResponse
    {
        public ItemCategoryResponse()
        {
            Subcategories = new List<ItemCategoryResponse>();
        }
        public int WebCategoryID { get; set; }
        public string WebCategoryDescription { get; set; }
        public int SortOrder { get; set; }
        public int? ParentID { get; set; }
        public int NestedLevel { get; set; }
        public List<ItemCategoryResponse> Subcategories { get; set; }
    }

   


}
