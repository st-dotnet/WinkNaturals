﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IItemCategory
    {
        int WebCategoryID { get; set; }
        string WebCategoryDescription { get; set; }
        int SortOrder { get; set; }
        int NestedLevel { get; set; }
        int? ParentID { get; set; }
        List<ItemCategory> Subcategories { get; set; }
    }
}
