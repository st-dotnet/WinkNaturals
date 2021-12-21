using System.Collections.Generic;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface ICustomerAutoOreder
    {
         IEnumerable<AutoOrder> GetCustomerAutoOrders(int customerid, int? autoOrderID = null, bool includePaymentMethods = true);
    }
}
