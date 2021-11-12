using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Infrastructure.Services.ExigoService
{
    public class OrderCalculationRequest
    {
        public int? CustomerID { get; set; }
        public IOrderConfiguration Configuration { get; set; }
        public IEnumerable<IShoppingCartItem> Items { get; set; }
        public IAddress Address { get; set; }
        public int OrderTypeID { get; set; }
        public int ShipMethodID { get; set; }
        public bool ReturnShipMethods { get; set; }
        public decimal? TaxRateOverride { get; set; }
        public Dictionary<string, decimal> ItemPriceOverrides { get; set; }
        public int? PartyID { get; set; }
        public string Other16 { get; set; } // Coupons
        public string Other17 { get; set; } // Points
        public string Other18 { get; set; } // HasSpecialItem
        public string Other20 { get; set; } // Enroll
    }
}
