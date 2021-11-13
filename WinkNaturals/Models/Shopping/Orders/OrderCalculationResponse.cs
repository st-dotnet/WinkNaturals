using Exigo.Api.Client;
using ExigoAPIRef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Orders
{
    public class OrderCalculationResponse
    {
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public List<OrderDetail> Details { get; set; }

        public IEnumerable<ShipMethod> ShipMethods { get; set; }

        public static implicit operator OrderCalculationResponse(Task<CalculateOrderResponse> v)
        {
            throw new NotImplementedException();
        }
    }
}
