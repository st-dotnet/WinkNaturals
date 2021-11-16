using Exigo.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Customer;
using WinkNatural.Web.Services.Interfaces;
using WinkNatural.Web.Services.Utilities;
using WinkNaturals.Models;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;

namespace WinkNatural.Web.Services.Services
{
    public class HomeService : IHomeService
    {
        private readonly string emailSubject = "Contact Us Email";
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IConfiguration _config;
        private readonly IOptions<ConfigSettings> _configSettings;

        public HomeService(IConfiguration config, IExigoApiContext exigoApiContext
            , IOptions<ConfigSettings> configSettings)
        {
            _config = config;
            _configSettings = configSettings;
            _exigoApiContext = exigoApiContext;
        }

        /// <summary>
        /// Get home page reviews from Yotpo api
        /// </summary>
        /// <returns></returns>
        public List<HomePageReviewsModel> GetReviews()
        {
            try
            {
                // Yotpo api url
                var url = $"{_configSettings.Value.YotPo.APIUrl}{_configSettings.Value.YotPo.ApiKey}/{_configSettings.Value.YotPo.HomePageEndpoints}";
                var client = new RestClient(url);
                #region Request

                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");

                #endregion

                #region Response

                IRestResponse response = client.Execute(request);
                dynamic dynamicReviews = JObject.Parse(response.Content);
                string jsonString = JsonConvert.SerializeObject(dynamicReviews.response.reviews);
                return JsonConvert.DeserializeObject<List<HomePageReviewsModel>>(jsonString);

                #endregion

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        /// <summary>
        /// Send contact email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ContactResponse> SendEmail(ContactRequest request)
        {
            try
            {
                //Get email template body
                var emailBody = EmailUtil.ContactEmailBody(request);
                var data = new SendEmailRequest
                {
                    Body = emailBody,
                    CustomerID = 0,
                    MailFrom = request.Email,
                    MailTo = _config.GetSection("EmailConfiguration:ContactUsEmail").Value,
                    Subject = emailSubject
                };

                //Send email from Exigo service
                var sendEmailRequest = await _exigoApiContext.GetContext().SendEmailAsync(data);
                return new ContactResponse { Success = true, ErrorMessage = null };
            }
            catch (Exception)
            {
                return new ContactResponse { Success = false, ErrorMessage = "Email not sent" };
            }
        }
    }
}
