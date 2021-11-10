using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;
using WinkNaturals.Setting;
using WinkNaturals.Utilities.Api.ThirdParty;
using WinkNaturals.Utilities.Common.YotPo.Domain;
using System;
using WinkNaturals.Helpers;

namespace WinkNaturals.Utilities.Common.YotPo
{
    public class YotPoApiService : IYotPoApiService
    {

        public YotPoApiService()
        {

        }
        private readonly IOptions<ConfigSettings> _config;
        public YotPoApiService(IOptions<ConfigSettings> config)
        {
            _config = config;
        }
        public AuthResponse GetAuthToken()
        {
            var url = _config.Value.YotPo.APIUrl + Apis.Oauth;    // GlobalSettings.YotPo.ApiBaseUrl + Apis.Oauth;

            var authRequest = new AuthRequest
            {
                ClientId = _config.Value.YotPo.ApiKey,
                ClientSecret = _config.Value.YotPo.ApiSecrete,
                GrantType = "client_credentials"
            };

            var response = PostHelpers.PostJson(url, authRequest.ToString());
            return JsonConvert.DeserializeObject<AuthResponse>(response);
        }

        public ServiceResponse PostOrder(PurchaseRequest purchase)
        {
            var serviceResponse = new ServiceResponse();
            var url = _config.Value.YotPo.ApiBaseUrl + Apis.FormattedPurchasesUrl.FormatWith(_config.Value.YotPo.ApiKey);
            try
            {
                var token = this.GetAuthToken();
                purchase.Utoken = token.AccessToken;
                var response = PostHelpers.PostJson(url, purchase.ToString());
                var purchaseResponse = JsonConvert.DeserializeObject<PurchaseResponse>(response);

                serviceResponse.IsSuccesful = purchaseResponse.Errors == null || purchaseResponse.Errors.Count == 0;
                serviceResponse.ResponseJson = response;
            }
            catch (System.Exception ex)
            {
                serviceResponse.IsSuccesful = false;
                serviceResponse.ResponseJson = "Message:" + ex.Message + "Stack Trace:" + ex.StackTrace;
            }
            return serviceResponse;
        }
    }
}
