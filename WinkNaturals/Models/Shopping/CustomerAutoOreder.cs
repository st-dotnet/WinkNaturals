using Dapper;
using Exigo.Api.Client;
using System.Collections.Generic;
using System.Linq;
using WinkNaturals.Infrastructure.Services.ExigoService;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;
using WinkNaturals.Infrastructure.Services.ExigoService.BankAccount;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.Interfaces;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Models.Shopping.Orders;
using WinkNaturals.Models.Shopping.PointAccount.Request;
using WinkNaturals.Setting.Interfaces;
using WinkNaturals.Utilities;
using static WinkNaturals.Helpers.Constant;
using static WinkNaturals.Models.Shopping.PointAccount.PointAccountRepo;
using BankAccountType = WinkNaturals.Helpers.Constant.BankAccountType;

namespace WinkNaturals.Models.Shopping
{
    public class CustomerAutoOreder : ICustomerAutoOreder
    {
        private readonly IExigoApiContext _exigoApiContext;
        public CustomerAutoOreder(IExigoApiContext exigoApiContext)
        {
            _exigoApiContext = exigoApiContext;
        }
        public IEnumerable<AutoOrder> GetCustomerAutoOrders(int customerid, int? autoOrderID = null, bool includePaymentMethods = true)
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

            var aoResponse = _exigoApiContext.GetContext(false).GetAutoOrdersAsync(request);//WebService().GetAutoOrders(request);

            if (aoResponse.Result.AutoOrders != null) return autoOrders;

            foreach (var aor in aoResponse.Result.AutoOrders)
            {
                autoOrders.Add((AutoOrder)aor);
            }

            // was getting all item  .Where(x => x.ParentItemCode == null)  maybe this is not needed?
            detailItemCodes = autoOrders.SelectMany(a => a.Details.Select(d => d.ItemCode)).Distinct().ToList();


            var autoOrderIds = autoOrders.Select(a => a.AutoOrderID).ToList();
            var createdDateNodes = new List<AutoOrderCreatedDate>();
            var aoDetailInfo = new List<AutoOrderDetailInfo>();

            using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())
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
                    detail.ImageUrl = GlobalUtilities.GetProductImagePath(detailInfo.ImageUrl);
                    detail.IsVirtual = detailInfo.IsVirtual;
                }
            }

            if (includePaymentMethods)
            {
                // Add payment methods
                var paymentMethods = GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
                {
                    CustomerID = customerid
                });

                foreach (var autoOrder in autoOrders)
                {
                    IPaymentMethod paymentMethod;
                    switch (autoOrder.AutoOrderPaymentTypeID)
                    {
                        case 1: paymentMethod = paymentMethods.Where(c => c is CreditCard && ((CreditCard)c).Type == CreditCardType.Primary).FirstOrDefault(); break;
                        case 2: paymentMethod = paymentMethods.Where(c => c is CreditCard && ((CreditCard)c).Type == CreditCardType.Secondary).FirstOrDefault(); break;
                        case 3: paymentMethod = paymentMethods.Where(c => c is BankAccount && ((BankAccount)c).Type == BankAccountType.Primary).FirstOrDefault(); break;
                        default: paymentMethod = null; break;
                    }
                    autoOrder.PaymentMethod = paymentMethod;
                }
            }

            return autoOrders;
        }
        public IEnumerable<IPaymentMethod> GetCustomerPaymentMethods(GetCustomerPaymentMethodsRequest request, IEnumerable<AutoOrder> autoOrders = null)
        {
            var methods = new List<IPaymentMethod>();
            //  if (!HttpContext.Request.IsAuthenticated) return methods.AsEnumerable();


            // Get the customer's billing info
            var billing = _exigoApiContext.GetContext(false).GetCustomerBillingAsync(new GetCustomerBillingRequest //DAL.WebService().GetCustomerBilling(new GetCustomerBillingRequest
            {
                CustomerID = request.CustomerID
            });


            if (autoOrders == null)
            {
                // Get the customer's auto orders
                //autoOrders =//await _exigoApiContext.GetContext().GetAutoOrdersAsync(new GetAutoOrdersRequest { CustomerID = request.CustomerID });
                //_ExigoService.DAL.GetCustomerAutoOrders(request.CustomerID, includePaymentMethods: false);
                autoOrders = GetCustomerAutoOrders(request.CustomerID, includePaymentMethods: false);
            }


            methods.Add(new BankAccount(BankAccountType.Primary)
            {
                BankName = string.Empty,
                NameOnAccount = billing.Result.BankAccount.NameOnAccount,
                AccountNumber = billing.Result.BankAccount.BankAccountNumberDisplay,
                RoutingNumber = billing.Result.BankAccount.BankRoutingNumber,
                AutoOrderIDs = autoOrders.Where(c => c.AutoOrderPaymentTypeID == AutoOrderPaymentTypes.DebitCheckingAccount).Select(c => c.AutoOrderID).ToArray(),

                BillingAddress = new Address()
                {
                    Address1 = billing.Result.BankAccount.BillingAddress,
                    City = billing.Result.BankAccount.BillingCity,
                    State = billing.Result.BankAccount.BillingState,
                    Zip = billing.Result.BankAccount.BillingZip,
                    Country = billing.Result.BankAccount.BillingCountry
                }
            });


            methods.Add(new CreditCard(CreditCardType.Primary)
            {
                NameOnCard = billing.Result.PrimaryCreditCard.BillingName,
                CardNumber = billing.Result.PrimaryCreditCard.CreditCardNumberDisplay,
                ExpirationMonth = billing.Result.PrimaryCreditCard.ExpirationMonth,
                ExpirationYear = billing.Result.PrimaryCreditCard.ExpirationYear,
                AutoOrderIDs = autoOrders.Where(c => c.AutoOrderPaymentTypeID == AutoOrderPaymentTypes.PrimaryCreditCardOnFile).Select(c => c.AutoOrderID).ToArray(),

                BillingAddress = new Address()
                {
                    Address1 = billing.Result.PrimaryCreditCard.BillingAddress,
                    City = billing.Result.PrimaryCreditCard.BillingCity,
                    State = billing.Result.PrimaryCreditCard.BillingState,
                    Zip = billing.Result.PrimaryCreditCard.BillingZip,
                    Country = billing.Result.PrimaryCreditCard.BillingCountry
                }
            });


            methods.Add(new CreditCard(CreditCardType.Secondary)
            {
                NameOnCard = billing.Result.SecondaryCreditCard.BillingName,
                CardNumber = billing.Result.SecondaryCreditCard.CreditCardNumberDisplay,
                ExpirationMonth = billing.Result.SecondaryCreditCard.ExpirationMonth,
                ExpirationYear = billing.Result.SecondaryCreditCard.ExpirationYear,
                AutoOrderIDs = autoOrders.Where(c => c.AutoOrderPaymentTypeID == AutoOrderPaymentTypes.SecondaryCreditCardOnFile).Select(c => c.AutoOrderID).ToArray(),

                BillingAddress = new Address()
                {
                    Address1 = billing.Result.SecondaryCreditCard.BillingAddress,
                    City = billing.Result.SecondaryCreditCard.BillingCity,
                    State = billing.Result.SecondaryCreditCard.BillingState,
                    Zip = billing.Result.SecondaryCreditCard.BillingZip,
                    Country = billing.Result.SecondaryCreditCard.BillingCountry
                }
            });


            // Filter out invalid or incomplete methods if applicable
            if (request.ExcludeInvalidMethods)
            {
                methods = methods.Where(c => c.IsValid).ToList();
            }
            if (request.ExcludeIncompleteMethods)
            {
                methods = methods.Where(c => c.IsComplete).ToList();
            }
            if (request.ExcludeNonAutoOrderPaymentMethods)
            {
                methods = methods.Where(c => c is IAutoOrderPaymentMethod).ToList();
            }


            return methods.AsEnumerable();
        }
    }
}

