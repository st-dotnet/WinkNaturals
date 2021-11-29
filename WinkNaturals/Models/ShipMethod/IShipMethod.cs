using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.ShipMethod
{
    public interface IShipMethod
    {
        int ShipMethodID { get; set; }
        string ShipMethodDescription { get; set; }
        decimal Price { get; set; }
        bool Selected { get; set; }
    }
}
