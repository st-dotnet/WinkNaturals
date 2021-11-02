using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Models
{
    public class OrderItemConfiguration : IOrderConfiguration
    {
        public int WarehouseID { get; set; }
        public string CurrencyCode { get; set; }
        public int PriceTypeID { get ; set; }
        public int LanguageID { get ; set; }
        public string DefaultCountryCode { get; set; }
        public int DefaultShipMethodID { get; set; }
        public int CategoryID { get; set; }
        public int FeaturedCategoryID { get; set; }
        public string Other16 { get; set; }
        public string Other17 { get; set; }
        public string Other18 { get; set; }
        public string Other20 { get; set; }
    }
}
