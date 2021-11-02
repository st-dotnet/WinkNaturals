using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.DTO.Shopping.CalculateOrder
{
   public class ShipMethod 
    {
        public int ShipMethodID { get; set; }
        public string Description { get; set; }
        public decimal ShippingAmount { get; set; }
        //public bool Selected { get; set; 
    }

    public interface IShipMethod
    {
        int ShipMethodID { get; set; }
        string ShipMethodDescription { get; set; }
        decimal Price { get; set; }
        bool Selected { get; set; }
    }
}
