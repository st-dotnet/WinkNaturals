using Exigo.Api.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Common.Utils;
using WinkNatural.Services.DTO.Customer;
using WinkNatural.Services.Interfaces;
using WinkNatural.Services.Utilities;

namespace WinkNatural.Services.Services
{
    public class HomeService : IHomeService
    {
        private readonly string emailSubject = "Contact Us Email";
        private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);

        private readonly IConfiguration _config;

        public HomeService(IConfiguration config)
        {
            _config = config;
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
                var sendEmailRequest = await exigoApiClient.SendEmailAsync(data);
                return new ContactResponse { Success = true, ErrorMessage = null };
            }
            catch (Exception)
            {
                return new ContactResponse { Success = false, ErrorMessage = "Email not sent" };
            }
        }
    }
}
