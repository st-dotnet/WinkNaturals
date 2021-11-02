using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;
using WinkNatural.Services.Interfaces;
using RestSharp;
using WinkNatural.Services.DTO.Shopping;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using static WinkNatural.Services.DTO.AuthPaymentModel;
using AutoMapper;

namespace WinkNatural.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        public PaymentService(IConfiguration config, ICustomerService customerService)
        {
            _config = config;
            _customerService = customerService;
        }

        public BankCardTransaction ProcessPayment(WinkPaymentRequest winkPaymentRequest)
        {
            BankCardTransaction bcx = new BankCardTransaction();

            bcx.setType(winkPaymentRequest.cardType);
            bcx.setCardName(winkPaymentRequest.cardName);
            bcx.setCardNumber(winkPaymentRequest.CardNumber);
            bcx.setCardExpirationMonth(winkPaymentRequest.CardExpirationMonth);
            bcx.setCardExpirationYear(winkPaymentRequest.CardExpirationYear);
            bcx.setAmount(winkPaymentRequest.Amount);
            BaseCommerceClient bcClient = new BaseCommerceClient("user", "pass", "key");

            bcx = bcClient.processBankCardTransaction(bcx);
            return bcx;
        }

        ProcessPaymentMethodTransactionResponse IPaymentService.ProcessPaymentMethod(GetPaymentRequest getPaymentProPayModel)
        {
            var baseUrl = "https://xmltestapi.propay.com/ProtectPay";
            var request = BuildRequest(getPaymentProPayModel);
            var pathAndQuery = string.Format("/Payers/{0}/PaymentMethods/ProcessedTransactions/", request.CardNumber);
            var restRequest = CreateRestRequest(pathAndQuery, RestSharp.Method.PUT);
            restRequest.AddBody(request);
            return Execute<ProcessPaymentMethodTransactionResponse>(restRequest, baseUrl);
        }


        public ProcessPaymentMethodTransactionRequest BuildRequest(GetPaymentRequest getPaymentProPayModel)
        {
            return new ProcessPaymentMethodTransactionRequest
            {
                Amount = getPaymentProPayModel.Price,
                CurrencyCode = "USD",
                CustomerId = getPaymentProPayModel.CustomerId,
                CardNumber = getPaymentProPayModel.CardNumber,
                PaymentMethodId = "411879be-1011-4ee3-8382-3598007769df,",
                ExpMonth = getPaymentProPayModel.ExpMonth,
                ExpYear = getPaymentProPayModel.ExpYear,
                CVV = getPaymentProPayModel.CVV,
                FullName = getPaymentProPayModel.FirstName,
                ZipCode = getPaymentProPayModel.ZipCode,
                Address1 = getPaymentProPayModel.Address1,
                City = getPaymentProPayModel.City,
                State = getPaymentProPayModel.State,
                Country = getPaymentProPayModel.Country,
            };
        }

        /// <summary>
        /// Request factory to ensure API key is always first parameter added.
        /// </summary>
        /// <param name="resource">The resource name.</param>
        /// <param name="method">The HTTP method.</param>
        /// <returns>Returns a new <see cref="RestRequest"/>.</returns>
        public RestRequest CreateRestRequest(string resource, Method method)
        {
            var restRequest = new RestRequest { Resource = resource, Method = method, RequestFormat = DataFormat.Json, };
            var credentials = GetCredentials();
            restRequest.AddHeader("accept", "application/json");
            restRequest.AddHeader("Authorization", credentials);
            return restRequest;
        }

        /// <summary>
        /// Gets the credentials for the call.
        /// </summary>
        /// <returns>The credentials.</returns>
        public string GetCredentials()
        {
            var billerAccountId = _config.GetSection("AppSettings:billerAccountId").Value;  // biller account id
            var authToken = _config.GetSection("AppSettings:authToken").Value;  // authentication token of biller
            var encodedCredentials = Convert.ToBase64String(Encoding.Default.GetBytes(billerAccountId + ":" + authToken));

            var credentials = string.Format("Basic {0}", encodedCredentials);
            return credentials;
        }


        private T Execute<T>(IRestRequest request, string baseUrl) where T : class, new()
        {
            var client = new RestClient(baseUrl);
            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                Console.WriteLine(
                "Error: Exception: {0}, Headers: {1}, Content: {2}, Status Code: {3}",
                response.ErrorException,
                response.Headers,
                response.Content,
                response.StatusCode);
            }

            return response.Data;
        }


        /// <summary>
        /// Request information for a call to the "ProcessPaymentMethodTransaction" method.
        /// </summary>
        public class ProcessPaymentMethodTransactionRequest
        {
            public int ExpMonth { get; set; }
            public int ExpYear { get; set; }
            public int CVV { get; set; }
            public string ZipCode { get; set; }
            public string FullName { get; set; }

            public string Address1 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }
            /// <summary>
            /// Gets or sets the payment method id.
            /// </summary>
            public string PaymentMethodId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the transaction is a recurring payment.
            /// </summary>
            public bool IsRecurringPayment { get; set; }

            /// <summary>
            /// Gets or sets the credit card Override values.
            /// </summary>
            public CreditCardOverrides CreditCardOverrides { get; set; }

            /// <summary>
            /// Gets or sets the ACH override values.
            /// </summary>
            public AchOverrides AchOverrides { get; set; }

            /// <summary>
            /// Gets or sets the payer override values.
            /// </summary>
            public PayerOverrides PayerOverrides { get; set; }
            /// <summary>
            /// Gets or sets the ID of the merchant profile to use for this transaction.
            /// </summary>
            /// <remarks>
            /// <para>Optional </para>
            /// Any 64-bit signed integer.
            /// If zero (default) is specified, the "default" merchant profile for the identity will be used.
            /// </remarks>
            public int CustomerId { get; set; }

            /// <summary>
            /// REQUIRED: Gets or sets the payer's card number.
            /// </summary>
            public string CardNumber { get; set; }

            /// <summary>
            /// REQUIRED: Gets or sets amount of the transaction.
            /// </summary>
            /// <remarks>
            /// <para>Amounts should be integers. No decimals are allowed.</para>
            /// </remarks>
            /// <example>
            /// <para>$ 0.50 = 50</para>
            /// <para>$ 1.50 = 150</para>
            /// <para>$100.00 = 10000</para>
            /// </example>
            public decimal Amount { get; set; }

            /// <summary>
            /// REQUIRED: Gets or sets the currency code for the transaction.
            /// </summary>
            /// <remarks>
            /// ISO 3166-1 3 digit country codes
            /// </remarks>
            /// <example>
            /// <para>840 - United States of America</para>
            /// <para>124 - Canada</para>
            /// </example>
            public string CurrencyCode { get; set; }

            /// <summary>
            /// Gets or sets the Invoice number for the transaction.
            /// </summary>
            /// <remarks>
            /// <para>Optional </para>
            /// <para>Length : 50</para>
            /// </remarks>
            public string Invoice { get; set; }

            /// <summary>
            /// Get or sets the comment 1.
            /// </summary>
            /// <remarks>
            /// <para>Optional </para>
            /// <para>Length : 128</para>
            /// </remarks>
            public string Comment1 { get; set; }

            /// <summary>
            /// Gets or sets the comment 2.
            /// </summary>
            /// <remarks>
            /// <para>Optional </para>
            /// <para>Length : 128</para>
            /// </remarks>
            public string Comment2 { get; set; }

            /// <summary>
            /// Gets or sets the debt repayment indicator from the request.
            /// </summary>
            public bool IsDebtRepayment { get; set; }
        }

        /// <summary>
        /// Credit Card Overrides.
        /// </summary>
        public class CreditCardOverrides
        {
            /// <summary>
            /// The Billing Address of the credit card.
            /// </summary>
            public Billing Billing
            {
                get; set;
            }

            /// <summary>
            /// The expiration date of the credit card.
            /// </summary>
            public string ExpirationDate
            {
                get; set;
            }

            /// <summary>
            /// The full name of the card holder.
            /// </summary>
            public string FullName
            {
                get; set;
            }

            /// <summary>
            /// The CVV for the card.
            /// </summary>
            public string CVV
            {
                get; set;
            }
        }

        /// <summary>
        /// Payers billing information.
        /// </summary>
        public class Billing
        {
            /// <summary>
            /// Address field 1.
            /// </summary>
            /// <remarks>
            /// <para>Length : 50</para>
            /// <para>Required</para>
            /// </remarks>
            public string Address1
            {
                get; set;
            }

            /// <summary>
            /// Address field 2.
            /// </summary>
            /// <remarks>
            /// <para>Length : 50</para>
            /// <para>Optional</para>
            /// </remarks>
            public string Address2
            {
                get; set;
            }

            /// <summary>
            /// Address field 3.
            /// </summary>
            /// <remarks>
            /// <para>Length : 50</para>
            /// <para>Optional</para>
            /// </remarks>
            public string Address3
            {
                get; set;
            }

            /// <summary>
            /// City.
            /// </summary>
            /// <remarks>
            /// <para>Length : 25</para>
            /// <para>Required</para>
            /// </remarks>
            public string City
            {
                get; set;
            }

            /// <summary>
            /// State or province.
            /// </summary>
            /// <remarks>
            /// <para>Length : 2</para>
            /// <para>Required</para>
            /// </remarks>
            /// <example>
            /// <para>"UT" for Utah</para>
            /// <para>"AB" for Alberta</para>
            /// </example>
            public string State
            {
                get; set;
            }

            /// <summary>
            /// Zip or Postal Code.
            /// </summary>
            /// <remarks>
            /// <para>Length : 10</para>
            /// <para>Required</para>
            /// </remarks>
            /// <example>
            /// <para>"84058" - Orem, Utah</para>
            /// <para>"T1X 1E1" - Calgary, Alberta</para>
            /// </example>
            public string ZipCode
            {
                get; set;
            }

            /// <summary>
            /// The country code.
            /// </summary>
            /// <remarks>
            /// <para>Required</para>
            /// <para>Note: See supported countries </para>
            /// </remarks>
            /// <example>
            /// <para>840 = United States of America</para>
            /// <para>124 = Canada</para>
            /// </example>
            public string Country
            {
                get; set;
            }

            /// <summary>
            /// The telephone number.
            /// </summary>
            public string TelephoneNumber
            {
                get; set;
            }

            /// <summary>
            /// The Email Address.
            /// </summary>
            public string Email
            {
                get; set;
            }
        }

        /// <summary>
        /// Overrides for ACH transactions.
        /// </summary>
        public class AchOverrides
        {
            /// <summary>
            /// The bank account type for the transaction (leave blank to use the type on file).
            /// </summary>
            public string BankAccountType
            {
                get; set;
            }

            /// <summary>
            /// The Standard Entry Class Code for the transaction.
            /// </summary>
            public string SecCode
            {
                get; set;
            }
        }

        /// <summary>
        /// Information about overrides pertaining to the payer.
        /// </summary>
        public class PayerOverrides
        {
            /// <summary>
            /// The IP Address of the user performing the transaction.
            /// </summary>
            public string IpAddress
            {
                get; set;
            }
        }

        /// <summary>
        /// The value returned from a call to the "ProcessPaymentMethodTransaction" method.
        /// </summary>

        /// <summary>
        /// <c>TransactionInformation</c> object for each transaction that was processed.
        /// </summary>
        public TransactionInformation Transaction
        {
            get; set;
        }

        /// <summary>
        /// <c>Result</c> structure for giving the result of the transaction.
        /// </summary>
        public Result RequestResult
        {
            get; set;
        }


        /// <summary>
        /// Transaction information from a attempt at processing a payment method in the ProPay system.
        /// </summary>
        public class TransactionInformation
        {
            /// <summary>
            /// Gets or sets transaction history id in the ProPay system.
            /// </summary>
            public string TransactionHistoryId { get; set; }

            /// <summary>
            /// Gets or sets authorization code from the system of record.
            /// </summary>
            public string AuthorizationCode { get; set; }

            /// <summary>
            /// Gets or sets address verification system (AVS) code.
            /// </summary>
            /// <remarks>
            /// Only present if billing information is present on a payment method and
            /// system of record supports AVS.
            /// </remarks>
            public string AVSCode { get; set; }

            /// <summary>
            /// Gets or sets the amount in the settled currency.
            /// </summary>
            public int CurrencyConvertedAmount { get; set; }

            /// <summary>
            /// Gets or sets the settled currency code.
            /// </summary>
            public string CurrencyConvertedCurrencyCode { get; set; }

            /// <summary>
            /// Gets or sets the conversion rate from the requested currency to the settled currency.
            /// </summary>
            public decimal CurrencyConversionRate { get; set; }

            /// <summary>
            /// Gets or sets gross amount in the settled currency.
            /// </summary>
            public virtual int? GrossAmt { get; set; }

            /// <summary>
            /// Gets or sets gross amount less net amount in the settled currency.
            /// </summary>
            public virtual int? GrossAmtLessNetAmt { get; set; }

            /// <summary>
            /// Gets or sets net amount in the settled currency.
            /// </summary>
            public virtual int? NetAmt { get; set; }

            /// <summary>
            /// Gets or sets per transaction fee in the settled currency.
            /// </summary>
            public virtual int? PerTransFee { get; set; }

            /// <summary>
            /// Gets or sets rate percentage.
            /// </summary>
            public virtual decimal? Rate { get; set; }

            /// <summary>
            /// Gets or sets specific result information from the transaction.
            /// </summary>
            public Result ResultCode { get; set; }

            /// <summary>
            /// Gets or sets transaction ID from the system of record.
            /// </summary>
            public string TransactionId { get; set; }

            /// <summary>
            /// <c>Gets or sets result</c> of the transaction.
            /// </summary>
            public string TransactionResult { get; set; }
        }

        /// <summary>
        /// The result of the call.
        /// </summary>
        public class Result
        {
            /// <summary>
            /// The result of the transaction
            /// </summary>
            /// <remarks>
            /// Will always be SUCCESS or FAILURE
            /// </remarks>
            public string ResultValue { get; set; }

            /// <summary>
            /// The result code of the transaction
            /// </summary>
            /// <remarks>
            /// Will be a two-digit string with only numbers. Allows "00" as a response.
            /// </remarks>
            public string ResultCode { get; set; }

            /// <summary>
            /// The english-text message of what went wrong (if anything)
            /// </summary>
            /// <remarks>
            /// The documenation show the empty string being returned in the success cases.
            /// </remarks>
            public string ResultMessage { get; set; }


        }

        public async Task<AddCardResponse> CreateCustomerProfile(GetPaymentRequest model)
        {
            var finalResponse = new AddCardResponse();
            var expMonth = model.ExpMonth < 10 ? $"0{model.ExpMonth}" : model.ExpMonth.ToString();
            string jsonData;

            var customer = _customerService.GetCustomer(model.CustomerId).Result.Customers[0];
            jsonData = JsonConvert.SerializeObject(new AuthorizeModel
            {
                createCustomerProfileRequest = new CreateCustomerProfileRequest
                {
                    merchantAuthentication = new DTO.MerchantAuthentication
                    {
                        name = _config.GetSection("AppSettings:APIKey").Value,
                        transactionKey = _config.GetSection("AppSettings:TransactionKey").Value,
                    },
                    profile = new DTO.Profile
                    {
                        description = $"This is a {customer.FirstName}'s Profile",
                        email = customer.Email,
                        paymentProfiles = new PaymentProfiles
                        {
                            customerType = "individual",
                            payment = new Payment
                            {
                                creditCard = new CreditCard
                                {
                                    cardNumber = model.CardNumber,
                                    expirationDate = $"20{model.ExpYear}-{model.ExpMonth}"
                                    
                                }
                            }
                        }
                    },
                    validationMode = "testMode"
                }
            });
            var stringContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri($"{_config.GetSection("AppSettings:AuthorizeNetTestBaseUrl").Value}createCustomerProfile");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.PostAsync("", stringContent).Result.Content.ReadAsStringAsync();
                var finalResult = JsonConvert.DeserializeObject<CreateCustomerProfileResponse>(response);
                finalResponse.Message = finalResult.messages.message.FirstOrDefault().text.ToString();
                finalResponse.ResultCode = finalResult.messages.resultCode;
            }
            catch (Exception ex)
            {

            }

            return finalResponse;
        }

        public async Task<AddCardResponse> AddPayment(AddPaymentModel model)
        {
            var finalResponse = new AddCardResponse();
            if (model.CreditCardId > 0)
            {
                var jsonData = JsonConvert.SerializeObject(new PaymentRequest
                {
                    createTransactionRequest = new CreateTransactionRequest
                    {
                        merchantAuthentication = new AuthPaymentModel.MerchantAuthentication
                        {
                            name = _config.GetSection("AppSettings:APIKey").Value,
                            transactionKey = _config.GetSection("AppSettings:TransactionKey").Value,
                        },
                        refId = "123456",
                        transactionRequest = new TransactionRequest
                        {
                            amount = model.PaymentAmount.ToString(),
                            lineItems = new LineItems
                            {
                                lineItem = new LineItem
                                {
                                    itemId = model.OrderId.ToString(),
                                    description = model.Description,
                                    name = "Test",
                                    quantity = model.quantity,   //"1",
                                    unitPrice = model.PaymentAmount.ToString()
                                }
                            },
                            transactionType = "authCaptureTransaction"
                        }
                    }

                });

                var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                try
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri($"{_config.GetSection("AppSettings:AuthorizeNetTestBaseUrl").Value}createCustomerProfile");
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.PostAsync("", stringContent).Result.Content.ReadAsStringAsync();
                    var finalResult = JsonConvert.DeserializeObject<AddPaymentResponse>(response);
                    finalResponse.Message = finalResult.messages.message.FirstOrDefault().text.ToString();
                    finalResponse.ResultCode = finalResult.messages.resultCode;
                    if (!string.IsNullOrEmpty(finalResult.transactionResponse.transId) && finalResult.messages.resultCode == "Ok")
                    {
                        model.Approved = true;
                        model.Result = $"{finalResult.transactionResponse.messages.FirstOrDefault().description}. The transaction Id is: {finalResult.transactionResponse.transId}";
                    }
                    else
                    {
                        model.Approved = false;
                        model.Result = $"{finalResult.messages.message.FirstOrDefault().text}. This transaction is failed";
                    }
                    
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                model.Approved = true;
                model.Result = $"This transaction is  approved  due to this is manual payment.";
            }
            return finalResponse;
        }


    }
}
