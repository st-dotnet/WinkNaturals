using Dapper;
using Exigo.Api.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Helpers;
using WinkNaturals.Infrastructure.Services.DTO;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Orders;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;
using WinkNaturals.Utilities.Common;
using static WinkNaturals.Helpers.Constant;
using Settings = WinkNaturals.Utilities.Common.Settings;

namespace WinkNatural.Web.Services.Services
{
    public class CustomerService : ICustomerService
    {
        // private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IOptions<ConfigSettings> _config;
        private readonly IEmailService _emailService;

        public CustomerService(IOptions<ConfigSettings> config, IEmailService emailService, IExigoApiContext exigoApiContext)
        {
            _emailService = emailService;
            _exigoApiContext = exigoApiContext;
            _config = config;
        }
        #region public methods

        public async Task<GetCustomersResponse> GetCustomer(int customerId)
        {
            try
            {
                return await _exigoApiContext.GetContext(false).GetCustomersAsync(new GetCustomersRequest { CustomerID = customerId });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<string> GetImage()
        {
            try
            {
                GetResourceSetCulturesRequest req = new GetResourceSetCulturesRequest();
                var aa = await _exigoApiContext.GetContext(false).GetResourceSetCulturesAsync(req);
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public Task<UpdateCustomerResponse> UpdateCustomer(UpdateCustomerRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion

        public async Task<bool> SendEmailVerification(int customerId, string email)
        {
            string emails = _config.Value.Emails.ToString();
            string sep = "&";
            if (!emails.Contains("?")) sep = "?";

            string encryptedValues = Settings.Encrypt(new
            {
                CustomerID = customerId,
                Email = email,
                Date = DateTime.Now
            });

            var verifyEmailUrl = Settings.Emails.VerifyEmailUrl + sep + "token=" + encryptedValues;


            // Send the email
            var response = await _emailService.Send(new EmailRequest
            {
                To = email,
                From = Settings.Emails.NoReplyEmail,
                Subject = "{0} - Verify your email".FormatWith(Settings.Company.Name),
                NoReply = _config.Value.EmailConfiguration.NoReplyEmail,
                Body = @"
                    <p>
                        {1} has received a request to enable this email account to receive email notifications from {1} and your upline.
                    </p>

                    <p> 
                        To confirm this email account, please click the following link:<br />
                        <a href='{0}'>{0}</a>
                    </p>

                    <p>
                        If you did not request email notifications from {1}, or believe you have received this email in error, please contact {1} customer service.
                    </p>

                    <p>
                        Sincerely, <br />
                        {1} Customer Service
                    </p>"
                    .FormatWith(verifyEmailUrl, Settings.Company.Name)
            });

            return response.Success;
        }

        //public async Task<Exigo.Api.Client.TransactionalResponse> ManageAutoOrder(ManageAutoOrderViewModel autoOrderViewModel, int id)
        //{
        //    try
        //    {
        //        int arraySize = 5;
        //        Exigo.Api.Client.TransactionalResponse response = new();
        //        Exigo.Api.Client.TransactionalRequest request = new()
        //        {
        //            TransactionRequests = new ITransactionMember[arraySize]
        //        };
        //        var customerID = id;
        //        var apiRequests = new List<ApiRequest>();
        //        var customer = _shoppingService.GetCustomer(customerID);
        //        var market = "US";
        //        var configuration = "US";
        //        var warehouseID = 1;
        //        var isExistingAutoOrder = id != 0;
        //        var paymentMethods = _accountService.GetCustomerBilling(id);

        //        autoOrderViewModel.AutoOrder.StartDate = autoOrderViewModel.AutoOrder.StartDate < DateTime.Now.ToCST() ? DateTime.Now.ToCST() : autoOrderViewModel.AutoOrder.StartDate;
        //        autoOrderViewModel.AutoOrder.Details = autoOrderViewModel.AutoOrder.Details.Where(d => d.Quantity > 0).ToList();
        //        if (!autoOrderViewModel.AutoOrder.Details.Any())
        //        {
        //        }
        //        //autoOrderViewModel.AvailableProducts = _shoppingService.GetItems(new GetItemsRequestAutoOrder()
        //        //{
        //        //    Configuration = "DS",
        //        //    LanguageID = _orderConfiguration.LanguageID,
        //        //    ItemCodes = autoOrderViewModel.AutoOrder.Details.Select(x => x.ItemCode).ToArray(),
        //        //}).OrderBy(c => c.SortOrder).ToList();
        //        foreach (var x in autoOrderViewModel.AutoOrder.Details)
        //        {
        //            if (!isExistingAutoOrder)
        //            {
        //                x.PriceEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                x.TaxableEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                x.ShippingPriceEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                x.BusinessVolumeEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.BV).FirstOrDefault();
        //                x.CommissionableVolumeEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.CV).FirstOrDefault();
        //            }
        //            else if (x.PriceEachOverride == null)
        //            {
        //                x.PriceEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                x.TaxableEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                x.ShippingPriceEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                x.BusinessVolumeEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.BV).FirstOrDefault();
        //                x.CommissionableVolumeEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.CV).FirstOrDefault();
        //            }
        //        }
        //        // Save New Credit Card
        //        var isUsingNewCard = autoOrderViewModel.AutoOrder.AutoOrderPaymentTypeID == 0;
        //        var hasPrimaryCard = paymentMethods.Result.Where(v => v.IsComplete).Count() > 0;
        //        if (isUsingNewCard)
        //        {
        //            var saveCCRequest = new SetAccountCreditCardTokenRequest();

        //            // If there is one or more available payment type, save the card in the secondary card slot
        //            if (hasPrimaryCard)
        //            {
        //                saveCCRequest.CreditCardAccountType = AccountCreditCardType.Secondary;
        //                autoOrderViewModel.AutoOrder.AutoOrderPaymentTypeID = AutoOrderPaymentTypes.SecondaryCreditCardOnFile;
        //            }
        //            else
        //            {
        //                autoOrderViewModel.AutoOrder.AutoOrderPaymentTypeID = AutoOrderPaymentTypes.PrimaryCreditCardOnFile;
        //            }
        //            saveCCRequest.CustomerID = customerID;
        //            request.TransactionRequests[4] = saveCCRequest;
        //        }
        //        // ToDo:  Joshua Remove after all users converted to TokenEx
        //        if (!hasPrimaryCard)
        //        {
        //            var updateCustomerRequest = new UpdateCustomerRequest
        //            {
        //                CustomerID = customerID,
        //                Field1 = "1"
        //            };
        //            var transactionResponse = _shoppingService.UpdateCustomer(updateCustomerRequest);
        //        }
        //        else
        //        {
        //            var updateCustomerRequest = new UpdateCustomerRequest
        //            {
        //                CustomerID = customerID,
        //                Field2 = "1"
        //            };
        //            var transactionResponse = _shoppingService.UpdateCustomer(updateCustomerRequest);
        //        }
        //        // Prepare the auto order
        //        var autoOrder = autoOrderViewModel.AutoOrder;
        //        var createAutoOrderRequest = new CreateAutoOrderRequest()
        //        {
        //            PriceType = 1,
        //            WarehouseID = warehouseID,
        //            Notes = !string.IsNullOrEmpty(autoOrder.Notes)
        //                            ? autoOrder.Notes
        //                            : string.Format("Created with the API Auto-Delivery manager at \"{0}\" on {1:u} at IP {2} using {3} {4} ({5}).",
        //                                DateTime.Now.ToUniversalTime()
        //                              ),
        //            CustomerID = customerID
        //        };
        //        request.TransactionRequests[3] = createAutoOrderRequest;

        //        request.TransactionRequests = request.TransactionRequests.Where(x => x != null).ToArray();

        //        // arraySize = Convert.ToInt32(request.TransactionRequests);
        //        //TransactionRequest
        //        response = await _exigoApiContext.GetContext(false).ProcessTransactionAsync(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.Message.ToString();
        //    }
        //    return null;
        //}

        //public object SaveNewCustomerCreditCard(int customerID, CreditCard card)
        //{
        //    // Get the credit cards on file
        //    var creditCardsOnFile = _accountService.GetCustomerBilling(customerID);

        //    // If no autoOrder-free slots exist, don't save it.
        //    return card;
        //}
        //public CreditCard SetCustomerCreditCard(int customerID, CreditCard card)
        //{
        //    return SetCustomerCreditCard(customerID, card, card.Type);
        //}
        //public CreditCard SetCustomerCreditCard(int customerID, CreditCard card, CreditCardType type)
        //{
        //    // New credit cards
        //    if (type == CreditCardType.New)
        //    {
        //        return (CreditCard)SaveNewCustomerCreditCard(customerID, card);
        //    }
        //    // Save the credit card
        //    var request = new SetAccountCreditCardTokenRequest
        //    {
        //        CustomerID = customerID,
        //        CreditCardAccountType = (card.Type == CreditCardType.Primary) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary,
        //        CreditCardToken = "41X111UAXYE31111",
        //        ExpirationMonth = card.ExpirationMonth,
        //        ExpirationYear = card.ExpirationYear,
        //        BillingName = card.NameOnCard,
        //        BillingAddress = card.BillingAddress.AddressDisplay,
        //        BillingCity = card.BillingAddress.City,
        //        BillingState = card.BillingAddress.State,
        //        BillingZip = card.BillingAddress.Zip,
        //        BillingCountry = card.BillingAddress.Country
        //    };
        //    var response = _exigoApiContext.GetContext(false).SetAccountCreditCardTokenAsync(request);//DAL.WebService().SetAccountCreditCardToken(request);
        //    return card;
        //}

        //public async Task<SetAccountResponse> DeleteCustomerCreditCard(int customerID, CreditCardType type)
        //{
        //    var res = new SetAccountResponse();

        //    // If this is a new credit card, don't delete it - we have nothing to delete
        //    if (type == CreditCardType.New) return res;
        //    // Save the a blank copy of the credit card
        //    // Passing a blank token will do the trick
        //    var request = new SetAccountCreditCardTokenRequest
        //    {
        //        CustomerID = customerID,
        //        CreditCardAccountType = (type == CreditCardType.Primary) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary,
        //        CreditCardToken = string.Empty,
        //        ExpirationMonth = 1,
        //        ExpirationYear = DateTime.Now.Year + 1,
        //        BillingName = string.Empty,
        //        BillingAddress = string.Empty,
        //        BillingCity = string.Empty,
        //        BillingState = string.Empty,
        //        BillingZip = string.Empty,
        //        BillingCountry = string.Empty
        //    };
        //    res = await _exigoApiContext.GetContext(false).SetAccountCreditCardTokenAsync(request);
        //    return res;
        //}
        //public async Task<SetAccountResponse> DeleteCustomerCreditCard(int customerID, CreditCardType type)
        //{
        //    var res = new SetAccountResponse();
        //    // If this is a new credit card, don't delete it - we have nothing to delete
        //    if (type == CreditCardType.New) return res;
        //    // Save the a blank copy of the credit card
        //    // Passing a blank token will do the trick
        //    var request = new SetAccountCreditCardTokenRequest
        //    {
        //        CustomerID = customerID,
        //        CreditCardAccountType = (type == CreditCardType.Primary) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary,
        //        CreditCardToken = string.Empty,
        //        ExpirationMonth = 1,
        //        ExpirationYear = DateTime.Now.Year + 1,
        //        BillingName = string.Empty,
        //        BillingAddress = string.Empty,
        //        BillingCity = string.Empty,
        //        BillingState = string.Empty,
        //        BillingZip = string.Empty,
        //        BillingCountry = string.Empty
        //    };
        //    res = await _exigoApiContext.GetContext(false).SetAccountCreditCardTokenAsync(request);
        //    return res;
        //}
        public async Task<SetAccountResponse> DeleteCustomerCreditCard(int customerID, string type)
        {
            var res = new SetAccountResponse();
            // If this is a new credit card, don't delete it - we have nothing to delete
            if (type ==CreditCardType.New.ToString()) return res;
            // Save the a blank copy of the credit card
            // Passing a blank token will do the trick
            var request = new SetAccountCreditCardTokenRequest
            {
                CustomerID = customerID,
                CreditCardAccountType = (type == CreditCardType.Primary.ToString()) ? AccountCreditCardType.Primary : AccountCreditCardType.Secondary,
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
            res = await _exigoApiContext.GetContext(false).SetAccountCreditCardTokenAsync(request);
            return res;
        }
        public async Task<ChangeAutoOrderStatusResponse> DeleteCustomerAutoOrder(int autoOrderID)
        {
            try
            {
                return await _exigoApiContext.GetContext(false).ChangeAutoOrderStatusAsync(new ChangeAutoOrderStatusRequest
                {
                    AutoOrderID = autoOrderID,
                    AutoOrderStatus = AutoOrderStatusType.Deleted
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            } 



            // Make sure the autoorder exists
            // if (!IsValidAutoOrderID(customerID, autoOrderID)) 
            //    return;


            //var response = await _exigoApiContext.GetContext(false).ChangeAutoOrderStatusAsync(new ChangeAutoOrderStatusRequest
            //{
            //    AutoOrderID = autoOrderID,
            //    AutoOrderStatus = AutoOrderStatusType.Deleted
            //});
        }
        private bool IsValidAutoOrderID(int customerID, int autoOrderID, bool showOnlyActiveAutoOrders = false)
        {
            var includeCancelled = "";
            if (showOnlyActiveAutoOrders)
            {
                includeCancelled = "AND a.AutoOrderStatusID = 0";
            }
            dynamic autoOrder;
            using (var context = Common.Utils.DbConnection.Sql())
            {
                autoOrder = context.Query<dynamic>(@"
                        SELECT
                        a.AutoOrderID
                        FROM
                        AutoOrders a
                        WHERE
                        a.CustomerID = @customerid
                        AND a.AutoOrderID = @autoorderid
                        " + includeCancelled, new
                        {
                            customerid = customerID,
                            autoorderid = autoOrderID
                        }).FirstOrDefault();
            }
            return autoOrder != null;
        }
    }
}

