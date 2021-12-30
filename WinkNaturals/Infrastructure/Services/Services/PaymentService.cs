using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.Token;
using WinkNaturals.Setting;

namespace WinkNatural.Web.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly ICustomerService _customerService;
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
            //Where we need to use expMonth?
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
                                creditCard = new DTO.CreditCard
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
                var client = new HttpClient
                {
                    BaseAddress = new Uri($"{_config.GetSection("AppSettings:AuthorizeNetTestBaseUrl").Value}createCustomerProfile")
                };
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.PostAsync("", stringContent).Result.Content.ReadAsStringAsync();
                var finalResult = JsonConvert.DeserializeObject<CreateCustomerProfileResponse>(response);
                finalResponse.Message = finalResult.messages.message.FirstOrDefault().text.ToString();
                finalResponse.ResultCode = finalResult.messages.resultCode;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return finalResponse;
        }
        public AddCardResponse PaymentUsingAuthorizeNet(AddPaymentModel addPaymentModel)
        {
            var finalResponse = new AddCardResponse();
            ApiOperationBase<AuthorizeNet.Api.Contracts.V1.ANetApiRequest, AuthorizeNet.Api.Contracts.V1.ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX; // define the merchant information (authentication / transaction id)
            ApiOperationBase<AuthorizeNet.Api.Contracts.V1.ANetApiRequest, AuthorizeNet.Api.Contracts.V1.ANetApiResponse>.MerchantAuthentication = new AuthorizeNet.Api.Contracts.V1.merchantAuthenticationType()
            {
                name = _configsetting.Value.AppSettings.APIKey,
                ItemElementName = AuthorizeNet.Api.Contracts.V1.ItemChoiceType.transactionKey,
                Item = _configsetting.Value.AppSettings.TransactionKey,
            };
            var creditCard = new AuthorizeNet.Api.Contracts.V1.creditCardType
            {
                cardNumber = addPaymentModel.CardNumber,
                expirationDate = addPaymentModel.ExpMonth + "" + addPaymentModel.ExpYear, //"1028",
                cardCode = addPaymentModel.CVV.ToString()
            }; var billingAddress = new AuthorizeNet.Api.Contracts.V1.customerAddressType
            {
                firstName = addPaymentModel.FirstName,
                address = addPaymentModel.Address1,
                city = addPaymentModel.City,
                zip = addPaymentModel.ZipCode
            }; //standard api call to retrieve response
            var paymentType = new AuthorizeNet.Api.Contracts.V1.paymentType { Item = creditCard }; var transactionRequest = new AuthorizeNet.Api.Contracts.V1.transactionRequestType
            {
                transactionType = AuthorizeNet.Api.Contracts.V1.transactionTypeEnum.authCaptureTransaction.ToString(), // charge the card
                amount = addPaymentModel.Price,
                payment = paymentType,
                billTo = billingAddress
            };
            var request = new AuthorizeNet.Api.Contracts.V1.createTransactionRequest { transactionRequest = transactionRequest }; // instantiate the controller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute(); // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse(); // validate response
            if (response != null)
            {
                if (response.messages.resultCode == AuthorizeNet.Api.Contracts.V1.messageTypeEnum.Ok)
                {
                    if (response.transactionResponse.messages != null)
                    {
                        finalResponse.TransId = response.transactionResponse.transId;
                        finalResponse.AuthCode = response.transactionResponse.authCode;
                    }
                    else
                    {
                        if (response.transactionResponse.errors != null)
                        {
                            finalResponse.TransId = response.transactionResponse.errors[0].errorCode;
                            finalResponse.AuthCode = "";
                        }
                    }
                }
                else
                {
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        finalResponse.TransId = response.transactionResponse.errors[0].errorCode;
                        finalResponse.AuthCode = "";
                    }
                    else
                    {
                        finalResponse.TransId = response.transactionResponse.errors[0].errorCode;
                        finalResponse.AuthCode = "";
                    }
                }
            }
            else
            {
                return new AddCardResponse { Message = "Some error occurred during the transaction" };
            }
            return finalResponse;
        }
        //Propay payment method
        public ProcessPaymentMethodTransactionResponse ProcessPaymentMethod(GetPaymentRequest getPaymentProPayModel)
        {
            //This is to get Payerid
            var client = new RestClient("https://xmltestapi.propay.com/ProtectPay/Payers/")
            {
                Timeout = -1
            };
            var request = new RestRequest(Method.PUT);
            var billerAccountId = _configsetting.Value.AppSettings.billerAccountId;// biller account id
            var authToken = _configsetting.Value.AppSettings.authToken;// authentication token of bille
            var encodedCredentials = Convert.ToBase64String(Encoding.Default.GetBytes(billerAccountId + ":" + authToken));
            var credentials = string.Format("Basic {0}", encodedCredentials);
            request.AddHeader("Authorization", credentials);
            request.AddHeader("Content-Type", "application/json");
            var body = @"{" + "\n" + @" ""Name"":#" + getPaymentProPayModel.FirstName + "#" + @"}";
            body = body.Replace('#', '"');
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var data = (JObject)JsonConvert.DeserializeObject(response.Content);
            string payerId = data["ExternalAccountID"].Value<string>();

            //This below is get PaymentMethodID
            var Payerclient = new RestClient("https://xmltestapi.propay.com/ProtectPay" + string.Format("/Payers/{0}/PaymentMethods/", payerId))
            {
                Timeout = -1
            };
            var requestpayer = new RestRequest(Method.PUT);
            requestpayer.AddHeader("Authorization", credentials);
            requestpayer.AddHeader("Content-Type", "application/json");
            var payerbody = @"{" + "\n" + @" ""PayerAccountId"":""2028688280138597""," + "\n" + @" ""PaymentMethodType"":#" + getPaymentProPayModel.CardType + "#" + "," + "\n" + @" ""AccountNumber"":#" + getPaymentProPayModel.AccountNo + "#" + "," + "\n" +
                            @" ""ExpirationDate"":#" + getPaymentProPayModel.ExpMonth + +getPaymentProPayModel.ExpYear + "#" + "," + @" ""AccountCountryCode"":""USA""," + "\n" +
                            @" ""AccountName"":#" + getPaymentProPayModel.FirstName + "#" + "," + @" ""BillingInformation"":" + "\n" +
                            @" {" + "\n" + @" ""Address1"":#" + getPaymentProPayModel.Address1 + "#" + "," +
                            @" ""City"":#" + getPaymentProPayModel.City + "#" + "," + "\n" +
                            @" ""Country"":#" + getPaymentProPayModel.Country + "#" + "," + "\n" + @" ""Email"":#" + getPaymentProPayModel.EmailAddress + "#" + "," + "\n" + @" ""State"":#" + getPaymentProPayModel.State + "#" + "," +
                            "\n" + @" ""TelephoneNumber"":null" + "," + "\n" + @" ""ZipCode"":#" + getPaymentProPayModel.ZipCode + "#" + "," + "\n" +
                            @" }," + "\n" + @" ""Description"":""MyVisaCard""," + "\n" + @" ""Priority"":0," + "\n" +
                            @" ""DuplicateAction"":null," + "\n" + @" ""Protected"":false" + "\n" + @"}";
            payerbody = payerbody.Replace('#', '"');
            requestpayer.AddParameter("application/json", payerbody, ParameterType.RequestBody);
            IRestResponse payerResponse = Payerclient.Execute(requestpayer);
            var payerdata = (JObject)JsonConvert.DeserializeObject(payerResponse.Content);
            string PaymentMethodId = payerdata["PaymentMethodId"].Value<string>();

            return new ProcessPaymentMethodTransactionResponse
            {
                Amount = getPaymentProPayModel.Price,
                CurrencyCode = "USD",
                CustomerId = getPaymentProPayModel.CustomerId,
                CardNumber = getPaymentProPayModel.CardNumber,
                PaymentMethodId = PaymentMethodId,
                ExpMonth = getPaymentProPayModel.ExpMonth,
                ExpYear = getPaymentProPayModel.ExpYear,
                CVV = getPaymentProPayModel.CVV,
                FullName = getPaymentProPayModel.FirstName,
                ZipCode = getPaymentProPayModel.ZipCode,
                Address1 = getPaymentProPayModel.Address1,
                City = getPaymentProPayModel.City,
                State = getPaymentProPayModel.State,
                Country = getPaymentProPayModel.Country,
                EmailAddress = getPaymentProPayModel.EmailAddress,
                ExternalId1 = getPaymentProPayModel.ExternalId1,
                ExternalId2 = getPaymentProPayModel.ExternalId2,
                AccountNo = getPaymentProPayModel.AccountNo
            };
        }

        public BankCardTransaction ProcessPayment(WinkPaymentRequest winkPaymentRequest)
        {
            throw new NotImplementedException();
        }

        public GenerateTokenResponse GenerateCreditCardToken(string cardNumber)
        {
            GenerateTokenResponse newtokenResponse = new GenerateTokenResponse();
            var client = new RestClient("https://test-api.tokenex.com/TokenServices.svc/REST/Tokenize");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            //  var body = @"{" + @" ""APIKey"":" + _configsetting.Value.TokenEx.APIKey + "," + @" ""TokenExID"":" + _configsetting.Value.TokenEx.TokenExID + "," + @" ""Data"":" + cardNumber + "," + @" ""TokenScheme"":" + _configsetting.Value.TokenEx.TokenScheme + @"}";
            //   var body = @"{ ""APIKey"": ""jvgxuIWt6aTlRA2rqgKIVNoow7BUxA9Mm1jnVwFh"", ""TokenExID"": ""3649316995937637"", ""Data"": ""2222405343248877"", ""TokenScheme"": 9 }";
            var body = @"{" + "\n" + @" ""APIKey"":#" + _configsetting.Value.TokenEx.APIKey + "#" + "," + @" ""TokenExID"":#" + _configsetting.Value.TokenEx.TokenExID + "#" + "," + "\n" +
                        @" ""Data"":#" + cardNumber + "#" + "," + "\n" + @" ""TokenScheme"":#" + _configsetting.Value.TokenEx.TokenScheme + "#" + "\n" + @"}";
            body = body.Replace('#', '"');
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var data = (JObject)JsonConvert.DeserializeObject(response.Content);
            string token = data["Token"].Value<string>();
            if (!string.IsNullOrEmpty(token))
            {
                StringBuilder sb = new StringBuilder(token);
                sb[2] = 'X';
                var newToken = sb.ToString();
                return new GenerateTokenResponse { Token = newToken, Success = true, ErrorMessage = "" };
            }
            else
            {
                return new GenerateTokenResponse { Token = null, Success = false, ErrorMessage = "error" };
            }
        }

    }
}
