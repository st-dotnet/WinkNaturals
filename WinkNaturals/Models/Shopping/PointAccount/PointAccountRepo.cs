using Dapper;
using Exigo.Api.Client;
using Microsoft.AspNetCore.Http;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.ExigoService;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;
using WinkNaturals.Infrastructure.Services.ExigoService.BankAccount;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.Interfaces;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces.PointAccount;
using WinkNaturals.Models.Shopping.Orders;
using WinkNaturals.Models.Shopping.PointAccount.Interfaces;
using WinkNaturals.Models.Shopping.PointAccount.Request;
using WinkNaturals.Setting.Interfaces;
using static WinkNaturals.Helpers.Constant;
using BankAccountType = WinkNaturals.Helpers.Constant.BankAccountType;

namespace WinkNaturals.Models.Shopping.PointAccount
{
    public class PointAccountRepo : ICustomerPointAccount
    {
        private readonly IExigoApiContext _exigoApiContext;
        private readonly ICustomerAutoOreder _customerAutoOreder;
        public PointAccountRepo(IExigoApiContext exigoApiContext, ICustomerAutoOreder customerAutoOreder)
        {
            _exigoApiContext = exigoApiContext;
            _customerAutoOreder = customerAutoOreder;
        }
        public object GetCustomerPointAccounts(int customerID, int pointAccountID)
        {
            var pointAccount = new CustomerPointAccount();
            using (var context = WinkNatural.Web.Common.Utils.DbConnection.Sql())
            {
                pointAccount = context.Query<CustomerPointAccount>(@"
                                SELECT cpa.PointAccountID
                                      , cpa.CustomerID
                                      , cpa.PointBalance AS Balance
	                                  , pa.PointAccountDescription
                                      , pa.CurrencyCode
                                FROM CustomerPointAccounts cpa
                                 LEFT JOIN PointAccounts pa
	                                ON cpa.PointAccountID = pa.PointAccountID
                                WHERE cpa.CustomerID = @CustomerID
                                    AND cpa.PointAccountID = @PointAccountID
                    ", new
                {
                    CustomerID = customerID,
                    PointAccountID = pointAccountID
                }).FirstOrDefault();
            }

            if (pointAccount == null)
                return null;

            return pointAccount;
        }

        public object SaveNewCustomerCreditCard(int customerID, CreditCard card)
        {
            // Get the credit cards on file
            var creditCardsOnFile = GetCustomerPaymentMethods(new GetCustomerPaymentMethodsRequest
            {
                CustomerID = customerID,
                ExcludeInvalidMethods = true
            }).Where(c => c is CreditCard).Select(c => (CreditCard)c);


            // Do we have any empty slots? If so, save this card to the next available slot
            if (!creditCardsOnFile.Any(c => c.Type == CreditCardType.Primary))
            {
                card.Type = CreditCardType.Primary;
                return SetCustomerCreditCard(customerID, card);
            }
            if (!creditCardsOnFile.Any(c => c.Type == CreditCardType.Secondary))
            {
                card.Type = CreditCardType.Secondary;
                return SetCustomerCreditCard(customerID, card);
            }


            // If not, try to save it to a card slot that does not have any autoOrder bound to it.
            if (!creditCardsOnFile.Where(c => c.Type == CreditCardType.Primary).Single().IsUsedInAutoOrders)
            {
                card.Type = CreditCardType.Primary;
                return SetCustomerCreditCard(customerID, card);
            }
            if (!creditCardsOnFile.Where(c => c.Type == CreditCardType.Secondary).Single().IsUsedInAutoOrders)
            {
                card.Type = CreditCardType.Secondary;
                return SetCustomerCreditCard(customerID, card);
            }


            // If no autoOrder-free slots exist, don't save it.
            return card;
        }
        public CreditCard SetCustomerCreditCard(int customerID, CreditCard card)
        {
            return SetCustomerCreditCard(customerID, card, card.Type);
        }
        public CreditCard SetCustomerCreditCard(int customerID, CreditCard card, CreditCardType type)
        {
            // New credit cards
            if (type == CreditCardType.New)
            {
                return (CreditCard)SaveNewCustomerCreditCard(customerID, card);
            }

            // Validate that we have a token
            var token = card.Token;     //card.GetToken();
            if (String.IsNullOrEmpty(token)) return card;


            // Save the credit card
            var request = new SetAccountCreditCardTokenRequest
            {
                CustomerID = customerID,

                CreditCardAccountType = (card.Type == CreditCardType.Primary) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary,
                CreditCardToken = token,
                ExpirationMonth = card.ExpirationMonth,
                ExpirationYear = card.ExpirationYear,

                BillingName = card.NameOnCard,
                BillingAddress = card.BillingAddress.AddressDisplay,
                BillingCity = card.BillingAddress.City,
                BillingState = card.BillingAddress.State,
                BillingZip = card.BillingAddress.Zip,
                BillingCountry = card.BillingAddress.Country
            };
            var response = _exigoApiContext.GetContext().SetAccountCreditCardTokenAsync(request);//DAL.WebService().SetAccountCreditCardToken(request);


            return card;
        }
        public IEnumerable<IPaymentMethod> GetCustomerPaymentMethods(GetCustomerPaymentMethodsRequest request, IEnumerable<AutoOrder> autoOrders = null)
        {
            var methods = new List<IPaymentMethod>();
          //  if (!HttpContext.Request.IsAuthenticated) return methods.AsEnumerable();


            // Get the customer's billing info
            var billing = _exigoApiContext.GetContext().GetCustomerBillingAsync(new GetCustomerBillingRequest //DAL.WebService().GetCustomerBilling(new GetCustomerBillingRequest
            {
                CustomerID = request.CustomerID
            });
            if (autoOrders == null)
            {
                // Get the customer's auto orders
                autoOrders = _customerAutoOreder.GetCustomerAutoOrders(request.CustomerID, includePaymentMethods: false);  //_ExigoService.DAL.GetCustomerAutoOrders(request.CustomerID, includePaymentMethods: false);
            }


            methods.Add(new BankAccount(BankAccountType.Primary)
            {
                BankName = string.Empty,
                NameOnAccount = billing.Result.BankAccount.
                BankAccountNumberDisplay = billing.Result.BankAccount.BankAccountNumberDisplay,
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
        public class AutoOrderCreatedDate
        {
            public int AutoOrderID { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class AutoOrderDetailInfo
        {
            public string ItemCode { get; set; }
            public bool IsVirtual { get; set; }
            public string ImageUrl { get; set; }
        }
        public static void DeleteCustomerCreditCard(int customerID, CreditCardType type)
        {
            // If this is a new credit card, don't delete it - we have nothing to delete
            if (type == CreditCardType.New) return;


            // Save the a blank copy of the credit card
            // Passing a blank token will do the trick
            var request = new SetAccountCreditCardTokenRequest
            {
                CustomerID = customerID,

                CreditCardAccountType = (type == CreditCardType.Primary) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary,
                CreditCardToken = string.Empty,
                ExpirationMonth = 1,
                ExpirationYear = DateTime.Now.Year + 1,

                BillingName = string.Empty,
                BillingAddress = string.Empty,
                BillingCity = string.Empty,
                BillingState = string.Empty,
                BillingZip = string.Empty,
                BillingCountry = string.Empty
            };
            // var response = DAL.WebService().SetAccountCreditCardToken(request);
        }
    }
}
