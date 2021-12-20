using Dapper;
using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.ExigoService;
using WinkNaturals.Infrastructure.Services.ExigoService.BankAccount;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Models.ShipMethod;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using WinkNaturals.Setting.Interfaces;
using static WinkNaturals.Helpers.Constant;
using BankAccountType = WinkNaturals.Helpers.Constant.BankAccountType;
using PointTransactionType = WinkNaturals.Models.PointTransactionType;

namespace WinkNaturals.Infrastructure.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly IShoppingService _shoppingService;
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IAuthenticateService _authenticateService;
        private readonly ICustomerService _customerService;

        public AccountService(IShoppingService shoppingService, IExigoApiContext exigoApiContext,IAuthenticateService authenticateService)
        {
            _shoppingService = shoppingService;
            _exigoApiContext = exigoApiContext;
            _authenticateService = authenticateService;
            
        }
        public IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, int pointAccountID)
        {
            var pointTransactions = new List<PointTransaction>();
            using (var context = DbConnection.Sql())
            {
                pointTransactions = context.Query<PointTransaction>(@"
                    select
	                    pt.PointTransactionID,
	                    pt.CustomerID,
	                    pt.PointAccountID,
	                    pt.Amount,
	                    pt.PointTransactionTypeID,
	                    pt.TransactionDate,
	                    pt.OrderID,
	                    pt.Reference
                    from PointTransactions pt
                    where 
	                    pt.CustomerID = @customerid
	                    and pt.PointAccountID = @pointaccountid
                    order by pt.TransactionDate desc", new
                {
                    customerid = customerID,
                    pointaccountid = pointAccountID
                }).ToList();
            }

            // If we have any, let's use other existing methods to get the accounts and types,
            // and we'll sync them here. This will allow us to use any custom logic or rules
            // in the other methods.
            if (pointTransactions.Any())
            {
                var accounts = GetLoyaltyPointAccounts().ToList();
                var types = GetPointTransactionTypes().ToList();

                foreach (var transaction in pointTransactions)
                {
                    transaction.PointAccount = accounts.FirstOrDefault(a => a.PointAccountID == transaction.PointAccountID);
                    transaction.PointTransactionType = types.FirstOrDefault(a => a.PointTransactionTypeID == transaction.PointTransactionTypeID);
                    if (transaction.Amount > 0)
                        transaction.ExpirationDate = transaction.TransactionDate.AddYears(2);
                    transaction.Balance = pointTransactions.Where(x => x.PointTransactionID <= transaction.PointTransactionID).Sum(x => x.Amount);
                }
            }
            if (pointTransactions == null) return null;

            return pointTransactions;
        }

        public IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, List<int> pointAccountIDs)
        {
            var pointTransactions = new List<PointTransaction>();
            using (var context = DbConnection.Sql())
            {
                pointTransactions = context.Query<PointTransaction>(@"
                    select
	                    pt.PointTransactionID,
	                    pt.CustomerID,
	                    pt.PointAccountID,
	                    pt.Amount,
	                    pt.PointTransactionTypeID,
	                    pt.TransactionDate,
	                    pt.OrderID,
	                    pt.Reference
                    from PointTransactions pt
                    where 
	                    pt.CustomerID = @customerid
	                    and pt.PointAccountID IN @pointaccountids
                ", new
                {
                    customerid = customerID,
                    pointaccountids = pointAccountIDs
                }).ToList();
            }

            // If we have any, let's use other existing methods to get the accounts and types,
            // and we'll sync them here. This will allow us to use any custom logic or rules
            // in the other methods.
            if (pointTransactions.Any())
            {
                var accounts = GetLoyaltyPointAccounts().ToList();
                var types = GetPointTransactionTypes().ToList();

                foreach (var transaction in pointTransactions)
                {
                    transaction.PointAccount = accounts.FirstOrDefault(a => a.PointAccountID == transaction.PointAccountID);
                    transaction.PointTransactionType = types.FirstOrDefault(a => a.PointTransactionTypeID == transaction.PointTransactionTypeID);
                }
            }


            if (pointTransactions == null) return null;

            return pointTransactions;
        }

        private PointAccount GetLoyaltyPointAccount(int pointAccountID)
        {
            var pointAccount = GetLoyaltyPointAccounts()
                  .Where(c => c.PointAccountID == pointAccountID)
                  .FirstOrDefault();
            if (pointAccount == null) return null;

            return (PointAccount)pointAccount;
        }

        private IEnumerable<PointAccount> GetLoyaltyPointAccounts()
        {
            var pointAccounts = new List<PointAccount>();
            using (var context = DbConnection.Sql())
            {
                pointAccounts = context.Query<PointAccount>(@"
                                SELECT PointAccountID
                                      , PointAccountDescription
                                      , CurrencyCode
                                FROM PointAccounts
                    ").ToList();
            }

            if (pointAccounts == null) return null;

            return pointAccounts;
        }
        public List<ShipMethodsResponse> GetShipMethodsRequest()
        {
            var shipMethods = new List<ShipMethodsResponse>();
            using (var context = DbConnection.Sql())
            {
                shipMethods = context.Query<ShipMethodsResponse>(@"
                                        SELECT 
                                            [ShipMethodID]
                                            ,[ShipMethodDescription]
                                            ,[WarehouseID]
                                            ,[ShipCarrierID]
                                            ,[DisplayOnWeb]
                                        FROM [dbo].[ShipMethods]
                                        WHERE DisplayOnWeb = 1"
                                       ).ToList();
            }
            return shipMethods;
        }

        public IEnumerable<PointTransaction> LoyaltyPointsService(int customerId, int pointAccountID)
        {
            try
            {
                var pointTransactions = new List<PointTransaction>();
                using (var context = DbConnection.Sql())
                {
                    pointTransactions = context.Query<PointTransaction>(@"
                    select
	                    pt.PointTransactionID,
	                    pt.CustomerID,
	                    pt.PointAccountID,
	                    pt.Amount,
	                    pt.PointTransactionTypeID,
	                    pt.TransactionDate,
	                    pt.OrderID,
	                    pt.Reference
                    from PointTransactions pt
                    where 
	                    pt.CustomerID = @customerid
	                    and pt.PointAccountID = @pointaccountid
                    order by pt.TransactionDate desc").ToList();
                }

                // If we have any, let's use other existing methods to get the accounts and types,
                // and we'll sync them here. This will allow us to use any custom logic or rules
                // in the other methods.
                if (pointTransactions.Any())
                {
                    var accounts = GetLoyaltyPointAccounts().ToList();
                    var types = GetPointTransactionTypes().ToList();

                    foreach (var transaction in pointTransactions)
                    {
                        transaction.PointAccount = accounts.FirstOrDefault(a => a.PointAccountID == transaction.PointAccountID);
                        transaction.PointTransactionType = types.FirstOrDefault(a => a.PointTransactionTypeID == transaction.PointTransactionTypeID);
                        if (transaction.Amount > 0)
                            transaction.ExpirationDate = transaction.TransactionDate.AddYears(2);
                        transaction.Balance = pointTransactions.Where(x => x.PointTransactionID <= transaction.PointTransactionID).Sum(x => x.Amount);
                    }
                }
                if (pointTransactions == null) return null;
                return pointTransactions;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public bool ValidateCustomerHasPointAmount(int customerID, int pointAccountID, decimal pointAmount)
        {
            var pointAccount = _shoppingService.GetCustomerLoyaltyPointAccount(customerID, pointAccountID);
            if (pointAccount == null) return false;
            return pointAccount.Balance >= pointAmount;
        }
        private List<PointTransactionType> GetPointTransactionTypes()
        {
            var pointTransactionTypes = new List<PointTransactionType>();
            using (var context = DbConnection.Sql())
            {
                pointTransactionTypes = context.Query<PointTransactionType>(@"
                SELECT PointTransactionTypeID
                        , PointTransactionTypeDescription
                FROM PointTransactionTypes
            ").ToList();
            }

            if (pointTransactionTypes == null) return null;

            return pointTransactionTypes;
        }

        public async Task<GetPointAccountResponse> CreatePointPayment(int customerId, int LoyaltyPointAccountId)
        {
            var res = new GetPointAccountResponse();
            try
            {
                var req = new GetPointAccountRequest();
                req.CustomerID = customerId;
                req.PointAccountID = LoyaltyPointAccountId;
                res = await _exigoApiContext.GetContext(false).GetPointAccountAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        public async Task<GetOrdersResponse> GetCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId)
        {
            //from order file
            var res = new GetOrdersResponse();
            try
            {
                var req = new GetOrdersRequest();
                req.CustomerID = customerID;
                res = await _exigoApiContext.GetContext(false).GetOrdersAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        public async Task<List<IPaymentMethod>> GetCustomerBilling(int customerId, GetAutoOrdersResponse autoOrders = null)
        {
            var methods = new List<IPaymentMethod>();
            var req = new GetCustomerBillingRequest();
            req.CustomerID = customerId;
            var response = await _exigoApiContext.GetContext(false).GetCustomerBillingAsync(req);
            if (autoOrders == null)
            {
                var request = new GetAutoOrdersRequest
                {
                    CustomerID = customerId,
                    AutoOrderStatus = AutoOrderStatusType.Active
                };
                // Get the customer's auto orders
                autoOrders = await _exigoApiContext.GetContext(false).GetAutoOrdersAsync(request);
            }
            methods.Add(new BankAccount(BankAccountType.Primary)
            {
                BankName = string.Empty,
                NameOnAccount = response.BankAccount.BillingAddress,
                AccountNumber = response.BankAccount.BankAccountNumberDisplay,
                RoutingNumber = response.BankAccount.BankRoutingNumber,
                AutoOrderIDs = autoOrders.AutoOrders.Where(c => c.PaymentType == AutoOrderPaymentType.CheckingAccount).Select(c => c.AutoOrderID).ToArray(),

                BillingAddress = new Address()
                {
                    Address1 = response.BankAccount.NameOnAccount,
                    City = response.BankAccount.BillingCity,
                    State = response.BankAccount.BillingState,
                    Zip = response.BankAccount.BillingZip,
                    Country = response.BankAccount.BillingCountry
                }
            });
            methods.Add(new CreditCard(CreditCardType.Primary)
            {
                NameOnCard = response.PrimaryCreditCard.BillingName,
                CardNumber = response.PrimaryCreditCard.CreditCardNumberDisplay,
                ExpirationMonth = response.PrimaryCreditCard.ExpirationMonth,
                ExpirationYear = response.PrimaryCreditCard.ExpirationYear,
                AutoOrderIDs = autoOrders.AutoOrders.Where(c => c.PaymentType == AutoOrderPaymentType.PrimaryCreditCard).Select(c => c.AutoOrderID).ToArray(),
                BillingAddress = new Address()
                {
                    Address1 = response.PrimaryCreditCard.BillingAddress,
                    City = response.PrimaryCreditCard.BillingCity,
                    State = response.PrimaryCreditCard.BillingState,
                    Zip = response.PrimaryCreditCard.BillingZip,
                    Country = response.PrimaryCreditCard.BillingCountry
                }

            });
            methods.Add(new CreditCard(CreditCardType.Secondary)
            {
                NameOnCard = response.SecondaryCreditCard.BillingName,
                CardNumber = response.SecondaryCreditCard.CreditCardNumberDisplay,
                ExpirationMonth = response.SecondaryCreditCard.ExpirationMonth,
                ExpirationYear = response.SecondaryCreditCard.ExpirationYear,
                AutoOrderIDs = autoOrders.AutoOrders.Where(c => c.PaymentType == AutoOrderPaymentType.SecondaryCreditCard).Select(c => c.AutoOrderID).ToArray(),
                BillingAddress = new Address()
                {
                    Address1 = response.SecondaryCreditCard.BillingAddress,
                    City = response.SecondaryCreditCard.BillingCity,
                    State = response.SecondaryCreditCard.BillingState,
                    Zip = response.SecondaryCreditCard.BillingZip,
                    Country = response.SecondaryCreditCard.BillingCountry
                }

            });
            return methods.ToList();
        }

        
    }
}