using Exigo.Api.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Services.DTO.Customer;
using WinkNatural.Web.Services.Interfaces;
using WinkNatural.Web.Services.Utilities;
using WinkNaturals.Setting.Interfaces;

namespace WinkNatural.Web.Services.Services
{
    public class HomeService : IHomeService
    {
        private readonly string emailSubject = "Contact Us Email";
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IConfiguration _config;

        public HomeService(IConfiguration config, IExigoApiContext exigoApiContext)
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
