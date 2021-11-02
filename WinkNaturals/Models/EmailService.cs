using Exigo.Api.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Models
{
    public class EmailService : IEmailService
    {
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IOptions<ConfigSettings> _settings;

        public EmailService(IExigoApiContext exigoApiContext, IOptions<ConfigSettings> settings)
        {
            _exigoApiContext = exigoApiContext;
            _settings = settings;
        }
        public async Task<EmailResponse> Send(EmailRequest request)
        {
            try
            {
                var sendEmailResponse = await _exigoApiContext.GetContext().SendEmailAsync(new SendEmailRequest
                {
                    CustomerID = request.CustomerId,
                    Body = request.Body,
                    MailFrom = _settings.Value.EmailConfiguration.NoReplyEmail,
                    MailTo = request.To,
                    Subject = request.Subject
                });

                return new EmailResponse
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new EmailResponse
                {
                    Success = false
                };
            }
        }
    }
}
