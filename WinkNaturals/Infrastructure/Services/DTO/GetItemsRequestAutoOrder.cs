using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Infrastructure.Services.DTO
{
    public class GetItemsRequestAutoOrder
    {
        public GetItemsRequestAutoOrder()
        {
            this.ItemCodes = new string[0];
            this.IncludeDynamicKitChildren = true;
        }

        public IOrderConfiguration Configuration { get; set; }
        public int? CategoryID { get; set; }
        public string[] ItemCodes { get; set; }
        public bool IncludeChildCategories { get; set; }
        public int PriceTypeID { get; set; }
        public int LanguageID { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int SortBy { get; set; }
        public bool IncludeDynamicKitChildren { get; set; }
        public bool IncludeLongDescriptions { get; set; }
        public bool IgnoreCache { get; set; }
    }
}