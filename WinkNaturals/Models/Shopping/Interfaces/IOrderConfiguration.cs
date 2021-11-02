using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Interfaces
{
   public interface IOrderConfiguration
    {
        int WarehouseID { get; set; }
        string CurrencyCode { get; set; }
        int PriceTypeID { get; set; }
        int LanguageID { get; set; }
        string DefaultCountryCode { get; set; }
        int DefaultShipMethodID { get; set; }
        int CategoryID { get; set; }
        int FeaturedCategoryID { get; set; }
        string Other16 { get; set; } // Coupons
        string Other17 { get; set; } // Points
        string Other18 { get; set; } // Has Special Item true or false
        string Other20 { get; set; } // Enroll
    }
}
