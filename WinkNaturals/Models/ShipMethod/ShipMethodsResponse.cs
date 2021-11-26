using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.ShipMethod
{
    public class ShipMethodsResponse : IShipMethod
    {
        public int ShipMethodID { get; set; }
        public string ShipMethodDescription { get; set; }
        public decimal Price { get; set; }
        public bool Selected { get; set; }
    }
}
