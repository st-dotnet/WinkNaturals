﻿using Dapper;
using Exigo.Api.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.ExigoService;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;
using WinkNaturals.Infrastructure.Services.ExigoService.BankAccount;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Models.ShipMethod;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;
using WinkNaturals.Utilities.Common;
using static WinkNaturals.Helpers.Constant;
using static WinkNaturals.Models.Shopping.PointAccount.PointAccountRepo;
using BankAccountType = WinkNaturals.Helpers.Constant.BankAccountType;
using PointTransactionType = WinkNaturals.Models.PointTransactionType;
using CreditCard = WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.CreditCard;

namespace WinkNaturals.Infrastructure.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly IShoppingService _shoppingService;
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IAuthenticateService _authenticateService;
        private readonly IOptions<ConfigSettings> _config;
        private readonly ICustomerAutoOreder _customerAutoOrder;

        public AccountService(IShoppingService shoppingService, IExigoApiContext exigoApiContext, IAuthenticateService authenticateService,IOptions<ConfigSettings> config, ICustomerAutoOreder customerAutoOrder)
        {
            _shoppingService = shoppingService;
            _exigoApiContext = exigoApiContext;
            _authenticateService = authenticateService;
            _config = config;
            _customerAutoOrder = customerAutoOrder;
        }
        public IEnumerable<PointTransaction> GetCustomerPointTransactions(int customerID, int pointAccountID)
        {
            var pointTransactions = new List<PointTransaction>();
            using (var context = DbConnection.Sql())
            {
                pointTransactions =   context.Query<PointTransaction>(@"
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
                var accounts =   GetLoyaltyPointAccounts().ToList();
                var types =   GetPointTransactionTypes().ToList();

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
                    order by pt.TransactionDate desc", new
                    {
                        customerid = customerId,
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
                else
                {
                    List<PointTransaction> points = new()
                    {
                        new()
                        {
                            PointTransactionID = 1,
                            Amount = 20,
                            Balance = 40,
                            CustomerID = customerId,
                            ExpirationDate = DateTime.Now.AddDays(10),
                            TransactionDate = DateTime.Now,
                            PointTransactionType = new()
                            {
                                PointTransactionTypeID = 1,
                                PointTransactionTypeDescription = "Adjustment"
                            },
                            OrderID = 5298,
                            PointTransactionTypeID = 1,
                        },
                        new()
                        {
                            PointTransactionID = 2,
                            Amount = 30,
                            Balance = 50,
                            CustomerID = customerId,
                            ExpirationDate = DateTime.Now.AddDays(15),
                            TransactionDate=DateTime.Now,
                            PointTransactionType = new()
                            {
                                PointTransactionTypeID = 2,
                                PointTransactionTypeDescription = "Point Redemption"
                            },
                            OrderID = 5299,
                            PointTransactionTypeID = 2,
                        },

                    };
                    pointTransactions.AddRange(points);
                }
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

        /// <summary>
        /// CancelledCustomerOrders_SQL
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="LoyaltyPointAccountId"></param>
        /// <returns></returns>
        public async Task<GetOrdersResponse> CancelledCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId)
        {
            //from order file
            var res = new GetOrdersResponse();
            try
            {
                var req = new GetOrdersRequest();
                req.CustomerID = customerID;
                req.OrderStatus = OrderStatusType.Canceled;


                res = await _exigoApiContext.GetContext(false).GetOrdersAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }
        /// <summary>
        /// SeachOrderList
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public async Task<GetOrdersResponse> SeachOrderList(int customerID, int orderid)
        {
            //from order file
            var res = new GetOrdersResponse();
            try
            {
                var req = new GetOrdersRequest();
                req.CustomerID = customerID;
                req.OrderID = orderid;
                res = await _exigoApiContext.GetContext(false).GetOrdersAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }
        /// <summary>
        /// DeclinedCustomerOrders_SQL
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="LoyaltyPointAccountId"></param>
        /// <returns></returns>
        public async Task<GetOrdersResponse> DeclinedCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId)
        {
            //from order file
            var res = new GetOrdersResponse();
            try
            {
                var req = new GetOrdersRequest();
                req.CustomerID = customerID;
                req.OrderStatus = OrderStatusType.CCDeclined;
                res = await _exigoApiContext.GetContext(false).GetOrdersAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }
        /// <summary>
        /// ShippedCustomerOrders_SQL
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="LoyaltyPointAccountId"></param>
        /// <returns></returns>
        public async Task<GetOrdersResponse> ShippedCustomerOrders_SQL(int customerID, int LoyaltyPointAccountId)
        {
            //from order file
            var res = new GetOrdersResponse();
            try
            {
                var req = new GetOrdersRequest();
                req.CustomerID = customerID;
                req.OrderStatus = OrderStatusType.Shipped;
                res = await _exigoApiContext.GetContext(false).GetOrdersAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }
        /// <summary>
        /// GetOrderInvoice
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GetOrderInvoiceResponse> GetOrderInvoice(int orderId)
        {
            //from order file
            var res = new GetOrderInvoiceResponse();
            try
            {
                var req = new GetOrderInvoiceRequest();
                req.OrderID = orderId;
                //req.OrderKey = request.OrderKey;
                req.ReportlayoutID = 1; //request.ReportlayoutID;
                req.Format = InvoiceRenderFormat.HTML;
            
                res = await _exigoApiContext.GetContext(false).GetOrderInvoiceAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }



        public async Task<IEnumerable<AutoOrder>> GetCustomerAutoOrders(int customerid, int? autoOrderID = null, bool includePaymentMethods = true)
        {
            var autoOrders = new List<AutoOrder>();
            var detailItemCodes = new List<string>();

            var request = new GetAutoOrdersRequest
            {
                CustomerID = customerid,
                AutoOrderStatus = AutoOrderStatusType.Active
            };

            if (autoOrderID != null)
            {
                request.AutoOrderID = (int)autoOrderID;
            }

            var aoResponse = await _exigoApiContext.GetContext(false).GetAutoOrdersAsync(request); // WebService().GetAutoOrders(request);

            if (aoResponse.AutoOrders == null)
                return autoOrders;

            foreach (var autoOrder in aoResponse.AutoOrders)
            {
                autoOrders.Add(new AutoOrder
                {
                    Details = autoOrder.Details.ToList(),
                    Address1 = autoOrder.Address1,
                    AutoOrderID = autoOrder.AutoOrderID,
                    ProcessType = autoOrder.ProcessType,
                    BVTotal = autoOrder.BusinessVolumeTotal,
                    CVTotal = autoOrder.CommissionableVolumeTotal,
                    CreatedDate = autoOrder.CreatedDate,
                    CurrencyCode = autoOrder.CurrencyCode,
                    CustomerID = autoOrder.CustomerID,
                    Description = autoOrder.Description,
                    Frequency = autoOrder.Frequency,
                    FrequencyTypeID = Convert.ToInt32(autoOrder.CustomFrequencyTy),
                    ShipMethodID = autoOrder.ShipMethodID,
                    Total = autoOrder.Total,
                    Subtotal = autoOrder.SubTotal,
                    TaxTotal = autoOrder.TaxTotal,
                    WarehouseID = autoOrder.WarehouseID,
                    AutoOrderPaymentTypeID = (int)autoOrder.PaymentType,
                    AutoOrderProcessTypeID = (int)autoOrder.ProcessType,
                    AutoOrderStatus = autoOrder.AutoOrderStatus
                });
            }
            // was getting all item.Where(x => x.ParentItemCode == null)  maybe this is not needed?
            detailItemCodes = autoOrders.SelectMany(a => a.Details.Select(d => d.ItemCode)).Distinct().ToList();


            var autoOrderIds = autoOrders.Select(a => a.AutoOrderID).ToList();
            var createdDateNodes = new List<AutoOrderCreatedDate>();
            var aoDetailInfo = new List<AutoOrderDetailInfo>();
            using (var context = DbConnection.Sql())
            {
                var nodeResults = context.QueryMultiple(@"
                        SELECT
                            AutoOrderID,
                            CreatedDate
                        FROM
                            AutoOrders
                        WHERE
                            AutoOrderID in @ids

                        SELECT
                            ItemCode,
                            SmallImageName,
                            IsVirtual
                        FROM Items
                        WHERE ItemCode in @itemcodes
                        ",
                    new
                    {
                        ids = autoOrderIds,
                        itemcodes = detailItemCodes
                    });

                createdDateNodes = nodeResults.Read<AutoOrderCreatedDate>().ToList();
                aoDetailInfo = nodeResults.Read<AutoOrderDetailInfo>().ToList();
            }
            foreach (var ao in autoOrders)
            {
                ao.CreatedDate = createdDateNodes.Where(n => n.AutoOrderID == ao.AutoOrderID).Select(n => n.CreatedDate).FirstOrDefault();

                foreach (var detail in ao.Details)
                {
                    var detailInfo = aoDetailInfo.Where(i => i.ItemCode == detail.ItemCode).FirstOrDefault();
                    // here I have added field with another refrence
                    detail.Reference1 = GetProductImagePath(detailInfo.ImageUrl);
                    //  detail.IsValid = detailInfo.IsVirtual;
                }
            }
            if (includePaymentMethods)
            {
                // Add payment methods
                var paymentMethods = _exigoApiContext.GetContext(false).GetAutoOrdersAsync(new GetAutoOrdersRequest
                {
                    CustomerID = customerid,
                });
                foreach (var autoOrder in autoOrders)
                {
                    IPaymentMethod paymentMethod;
                    switch (autoOrder.AutoOrderPaymentTypeID)
                    {

                        case 1: paymentMethod = paymentMethods.Result.AutoOrders.Where(c => c.PaymentType == AutoOrderPaymentType.PrimaryCreditCard).FirstOrDefault() as IPaymentMethod; break;
                        case 2: paymentMethod = paymentMethods.Result.AutoOrders.Where(c => c.PaymentType == AutoOrderPaymentType.SecondaryCreditCard).FirstOrDefault() as IPaymentMethod; break;
                        case 3: paymentMethod = paymentMethods.Result.AutoOrders.Where(c => c.PaymentType == AutoOrderPaymentType.CheckingAccount).FirstOrDefault() as IPaymentMethod; break;
                        default: paymentMethod = null; break;
                    }
                    autoOrder.PaymentMethod = paymentMethod;
                }
            }
            return autoOrders;
        }
        private string GetProductImagePath(string productImage)
        {
            productImage = productImage ?? string.Empty;
            if (String.IsNullOrEmpty(productImage) || productImage.Contains("nopic.gif"))
            {
               // string imageUrl =new Uri(_config.Value.Company.BaseReplicatedUrl + productImage).AbsoluteUri;
                return "/content/images/missing-product-image.png";
                //   return "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAmwAAAJsCAMAAAB3Sw8XAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyJpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDUuMC1jMDYwIDYxLjEzNDc3NywgMjAxMC8wMi8xMi0xNzozMjowMCAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIENTNSBNYWNpbnRvc2giIHhtcE1NOkluc3RhbmNlSUQ9InhtcC5paWQ6MEZENEZGNjlDRjBDMTFFMUE1MEJENUIxN0JDMzkwQjkiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6MEZENEZGNkFDRjBDMTFFMUE1MEJENUIxN0JDMzkwQjkiPiA8eG1wTU06RGVyaXZlZEZyb20gc3RSZWY6aW5zdGFuY2VJRD0ieG1wLmlpZDowRkQ0RkY2N0NGMEMxMUUxQTUwQkQ1QjE3QkMzOTBCOSIgc3RSZWY6ZG9jdW1lbnRJRD0ieG1wLmRpZDowRkQ0RkY2OENGMEMxMUUxQTUwQkQ1QjE3QkMzOTBCOSIvPiA8L3JkZjpEZXNjcmlwdGlvbj4gPC9yZGY6UkRGPiA8L3g6eG1wbWV0YT4gPD94cGFja2V0IGVuZD0iciI/PoVgV5IAAAFiUExURf///6ampqCg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";
               // return imageUrl;
                    }
            else
            {
                if (productImage.ToLower().StartsWith("http"))
                    return productImage;
                else
                    return "{0}{1}".FormatWith(GetProductImagePath(), productImage);
            }
        }
        private string GetProductImagePath()
        {
            return "/shopping/productimages/";
        }

        public async Task<GetAutoOrdersResponse> GetCustomerAutoOrdersList(int customerId)
        {
            var req = new GetAutoOrdersRequest();
            req.CustomerID = customerId;
            req.AutoOrderStatus = AutoOrderStatusType.Active;
            var res = await _exigoApiContext.GetContext(false).GetAutoOrdersAsync(req);
            return res;
        }
        public async Task<WinkNatural.Web.Services.DTO.Shopping.Address> MakeAddressAsPrimary(int customerId, WinkNatural.Web.Services.DTO.Shopping.Address address)
        {
            try
            {
                var request = new UpdateCustomerRequest
                {
                    CustomerID = customerId,
                    MainAddress1 = address.Address1,
                    MainAddress2 = address.Address2 ?? string.Empty,
                    MainCity = address.City,
                    MainState = address.State,
                    MainZip = address.Zip,
                    MainCountry = address.Country
                };
                await _exigoApiContext.GetContext(false).UpdateCustomerAsync(request);
                return address;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// MakeAddressAsPrimary
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>







        //#region Save cradit card

        //public async Task<CreditCard> SetCustomerCreditCard(int customerID, CreditCard card)
        //{
        //    return await SetCustomerCreditCard(customerID, card, card.Type);
        //}

        //private async Task<CreditCard> SetCustomerCreditCard(int customerID, CreditCard card, CreditCardType type)
        //{
        //    // New credit cards
        //    if (type == CreditCardType.New)
        //    {
        //        return await SaveNewCustomerCreditCard(customerID, card);
        //    }

        //    // Validate that we have a token
        //    var token = card.Token;     //card.GetToken();
        //    if (string.IsNullOrEmpty(token)) return card; 

        //    // Save the credit card
        //    var request = new SetAccountCreditCardTokenRequest
        //    {
        //        CustomerID = customerID,
        //        CreditCardAccountType = (card.Type == CreditCardType.Primary) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary,
        //        CreditCardToken = token,
        //        ExpirationMonth = card.ExpirationMonth,
        //        ExpirationYear = card.ExpirationYear,

        //        BillingName = card.NameOnCard,
        //        BillingAddress = card.BillingAddress.AddressDisplay,
        //        BillingCity = card.BillingAddress.City,
        //        BillingState = card.BillingAddress.State,
        //        BillingZip = card.BillingAddress.Zip,
        //        BillingCountry = card.BillingAddress.Country
        //    };
        //    var response = _exigoApiContext.GetContext(false).SetAccountCreditCardTokenAsync(request); 
        //    return card;
        //}

        //private async Task<CreditCard> SaveNewCustomerCreditCard(int customerID, CreditCard card)
        //{
        //    // Get the credit cards on file
        //    var creditCardsOnFile = GetCustomerBilling(customerID).Result.Where(c=>c is CreditCard).Select(c => (CreditCard)c);

        //    // Do we have any empty slots? If so, save this card to the next available slot
        //    if (!creditCardsOnFile.Any(c => c.Type == CreditCardType.Primary))
        //    {
        //        card.Type = CreditCardType.Primary;
        //        return await SetCustomerCreditCard(customerID, card);
        //    }
        //    if (!creditCardsOnFile.Any(c => c.Type == CreditCardType.Secondary))
        //    {
        //        card.Type = CreditCardType.Secondary;
        //        return await SetCustomerCreditCard(customerID, card);
        //    }


        //    // If not, try to save it to a card slot that does not have any autoOrder bound to it.
        //    if (!creditCardsOnFile.Where(c => c.Type == CreditCardType.Primary).Single().IsUsedInAutoOrders)
        //    {
        //        card.Type = CreditCardType.Primary;
        //        return await SetCustomerCreditCard(customerID, card);
        //    }
        //    if (!creditCardsOnFile.Where(c => c.Type == CreditCardType.Secondary).Single().IsUsedInAutoOrders)
        //    {
        //        card.Type = CreditCardType.Secondary;
        //        return await SetCustomerCreditCard(customerID, card);
        //    } 

        //    // If no autoOrder-free slots exist, don't save it.
        //    return card;
        //}
        //#endregion
    }
}
