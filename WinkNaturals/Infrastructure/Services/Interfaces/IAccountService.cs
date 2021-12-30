﻿
using Exigo.Api.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Models;
using WinkNaturals.Models.ShipMethod;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Infrastructure.Services.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<PointTransaction> LoyaltyPointsService(int customerId, int LoyaltyPointAccountId);
        List<ShipMethodsResponse> GetShipMethodsRequest();
        IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, int pointAccountID);
        IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, List<int> pointAccountIDs);
        bool ValidateCustomerHasPointAmount(int customerID, int pointAccountID, decimal pointAmount);
        Task<GetPointAccountResponse> CreatePointPayment(int customerId, int LoyaltyPointAccountId);
        Task<GetOrdersResponse> GetCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId);
        Task<List<IPaymentMethod>> GetCustomerBilling(int customerId,GetAutoOrdersResponse autoOrders = null);
        Task<IEnumerable<AutoOrder>> GetCustomerAutoOrders(int customerid, int? autoOrderID = null, bool includePaymentMethods = true);
        Task<GetOrdersResponse> CancelledCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId);
        Task<GetOrdersResponse> SeachOrderList(int customerID, int orderid);
        Task<GetOrdersResponse> DeclinedCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId);
        Task<GetOrdersResponse> ShippedCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId);
        Task<GetOrderInvoiceResponse> GetOrderInvoice(int orderId);
        //  Task<TransactionalResponse> ManageAutoOrder(ManageAutoOrderViewModel autoOrderViewModel, int id);

        Task<GetAutoOrdersResponse> GetCustomerAutoOrdersList(int customerid);

        //Save cradit card
        // Task<CreditCard> SetCustomerCreditCard(int customerID, CreditCard card);
        Task<Address> MakeAddressAsPrimary(int customerId, Address address);

        //Make credit card primary
        Task<bool> MakeCreditCardAsPrimary(int customerID, CreditCard card, CreditCardType type);

        Task<GetAutoOrdersResponse> EditSubcription(int customerId, int autoOrderId);
    }
}
