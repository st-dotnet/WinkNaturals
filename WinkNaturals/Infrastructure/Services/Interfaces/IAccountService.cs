
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models;
using WinkNaturals.Models.ShipMethod;
using WinkNaturals.Models.Shopping.Interfaces.PointAccount;

namespace WinkNaturals.Infrastructure.Services.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<PointTransaction> LoyaltyPointsService(int customerId,int LoyaltyPointAccountId);

       
        List<ShipMethodsResponse> GetShipMethodsRequest();
        IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, int pointAccountID);
        IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, List<int> pointAccountIDs);
        bool ValidateCustomerHasPointAmount(int customerID, int pointAccountID, decimal pointAmount);
    }
}
