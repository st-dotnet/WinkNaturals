using System.Collections.Generic;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Models
{
    public class ItemCategory : IItemCategory
    {
        public ItemCategory()
        {
            Subcategories = new List<ItemCategory>();
        }

        public int WebCategoryID { get; set; }
        public string WebCategoryDescription { get; set; }
        public int SortOrder { get; set; }
        public int? ParentID { get; set; }
        public int NestedLevel { get; set; }
        public List<ItemCategory> Subcategories { get; set; }
    }
}
