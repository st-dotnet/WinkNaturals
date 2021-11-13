using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;

namespace WinkNaturals.Models.Shopping.Interfaces
{
  public  interface ICustomerAutoOreder
    {
        public IEnumerable<AutoOrder> GetCustomerAutoOrders(int customerid, int? autoOrderID = null, bool includePaymentMethods = true);
    }
}
