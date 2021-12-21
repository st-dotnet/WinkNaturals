﻿
using Exigo.Api.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.ExigoService;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;
using WinkNaturals.Models;
using WinkNaturals.Models.ShipMethod;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;

namespace WinkNaturals.Infrastructure.Services.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<PointTransaction> LoyaltyPointsService(int customerId,int LoyaltyPointAccountId);
        List<ShipMethodsResponse> GetShipMethodsRequest();
        IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, int pointAccountID);
        IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, List<int> pointAccountIDs);
        bool ValidateCustomerHasPointAmount(int customerID, int pointAccountID, decimal pointAmount);
        Task<GetPointAccountResponse> CreatePointPayment(int customerId, int LoyaltyPointAccountId);
        Task<GetOrdersResponse> GetCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId);
        Task<List<IPaymentMethod>> GetCustomerBilling(int customerId,GetAutoOrdersResponse autoOrders = null);
      //  List<GetAutoOrdersResponse> GetCustomerAutoOrders(int customerid, int? autoOrderID = null, bool includePaymentMethods = true);
    }
}
