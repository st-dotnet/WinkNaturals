using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Infrastructure.Services.ExigoService.Items.Requests
{
    public class GetItemsRequest
    {
        public GetItemsRequest()
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