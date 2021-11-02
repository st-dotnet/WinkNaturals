using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.Interfaces;
using RestSharp;
using WinkNatural.Web.Services.DTO.Shopping;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using static WinkNatural.Web.Services.DTO.AuthPaymentModel;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using WinkNaturals.Setting;

namespace WinkNatural.Web.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;
        private readonly IOptions<ConfigSettings> _configsetting;
        public PaymentService(IConfiguration config, ICustomerService customerService, IOptions<ConfigSettings> configsetting)
        {
            _config = config;
            _customerService = customerService;
            _configsetting = configsetting;
        }


        /// <summary>
        /// Request information for a call to the "ProcessPaymentMethodTransaction" method.
        /// </summary>
    

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

        //Propay payment method
        public ProcessPaymentMethodTransactionResponse ProcessPaymentMethod(GetPaymentRequest getPaymentProPayModel)
        {
            //This is to get Payerid
            var client = new RestClient("https://xmltestapi.propay.com/ProtectPay/Payers/");
            client.Timeout = -1;
            var request = new RestRequest(Method.PUT);
            var billerAccountId = _configsetting.Value.AppSettings.billerAccountId; //_config.GetSection("AppSettings:billerAccountId").Value; // biller account id
            var authToken = _configsetting.Value.AppSettings.authToken;//_config.GetSection("AppSettings:authToken").Value; // authentication token of bille
            var encodedCredentials = Convert.ToBase64String(Encoding.Default.GetBytes(billerAccountId + ":" + authToken));
            var credentials = string.Format("Basic {0}", encodedCredentials);
            request.AddHeader("Authorization", credentials);
            request.AddHeader("Content-Type", "application/json");
            var body = @"{" + "\n" + @" ""Name"":#" + getPaymentProPayModel.FirstName + "#" + @"}";
            body = body.Replace('#', '"');
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            //Console.WriteLine(response.Content);
            var data = (JObject)JsonConvert.DeserializeObject(response.Content);
            string payerId = data["ExternalAccountID"].Value<string>();

            //This below is get PaymentMethodID
            var Payerclient = new RestClient("https://xmltestapi.propay.com/ProtectPay" + string.Format("/Payers/{0}/PaymentMethods/", payerId));
            Payerclient.Timeout = -1;
            var requestpayer = new RestRequest(Method.PUT);
            requestpayer.AddHeader("Authorization", credentials);
            requestpayer.AddHeader("Content-Type", "application/json");
            var payerbody = @"{" + "\n" + @" ""PayerAccountId"":""2028688280138597""," + "\n" +
            @" ""PaymentMethodType"":""Visa""," + "\n" + @" ""AccountNumber"":""4111111111111111""," +
            "\n" +
            @" ""ExpirationDate"":""0819""," + "\n" + @" ""AccountCountryCode"":""USA""," + "\n" +
            @" ""AccountName"":""Janis Joplin""," + "\n" + @" ""BillingInformation"":" + "\n" +
            @" {" + "\n" + @" ""Address1"":""123 ABC St""," + "\n" + @" ""Address2"":""Apt. A""," +
            "\n" + @" ""Address3"":null," + "\n" + @" ""City"":""Some Place""," + "\n" +
            @" ""Country"":""USA""," + "\n" + @" ""Email"":null," + "\n" + @" ""State"":""AK""," +
            "\n" + @" ""TelephoneNumber"":null," + "\n" + @" ""ZipCode"":""12345""" + "\n" +
            @" }," + "\n" + @" ""Description"":""MyVisaCard""," + "\n" + @" ""Priority"":0," + "\n" +
            @" ""DuplicateAction"":null," + "\n" + @" ""Protected"":false" + "\n" +
            @"}";
            requestpayer.AddParameter("application/json", payerbody, ParameterType.RequestBody);
            IRestResponse payerResponse = Payerclient.Execute(requestpayer);
            var payerdata = (JObject)JsonConvert.DeserializeObject(payerResponse.Content);
            string PaymentMethodId = payerdata["PaymentMethodId"].Value<string>();

            ProcessPaymentMethodTransactionResponse responseObj = new ProcessPaymentMethodTransactionResponse();
            responseObj.Amount = getPaymentProPayModel.Price;
            responseObj.CurrencyCode = "USD";
            responseObj.CustomerId = getPaymentProPayModel.CustomerId;
            responseObj.CardNumber = getPaymentProPayModel.CardNumber;
            responseObj.PaymentMethodId = PaymentMethodId;
            responseObj.ExpMonth = getPaymentProPayModel.ExpMonth;
            responseObj.ExpYear = getPaymentProPayModel.ExpYear;
            responseObj.CVV = getPaymentProPayModel.CVV;
            responseObj.FullName = getPaymentProPayModel.FirstName;
            responseObj.ZipCode = getPaymentProPayModel.ZipCode;
            responseObj.Address1 = getPaymentProPayModel.Address1;
            responseObj.City = getPaymentProPayModel.City;
            responseObj.State = getPaymentProPayModel.State;
            responseObj.Country = getPaymentProPayModel.Country;
            responseObj.EmailAddress = getPaymentProPayModel.EmailAddress;
            responseObj.ExternalId1 = getPaymentProPayModel.ExternalId1;
            responseObj.ExternalId2 = getPaymentProPayModel.ExternalId2;
            return responseObj;
        }

        public BankCardTransaction ProcessPayment(WinkPaymentRequest winkPaymentRequest)
        {
            throw new NotImplementedException();
        }
    }
}
