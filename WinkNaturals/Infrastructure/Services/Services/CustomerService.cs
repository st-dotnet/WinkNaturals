﻿using Exigo.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;
using WinkNaturals.Utilities.Common;
using Settings = WinkNaturals.Utilities.Common.Settings;

namespace WinkNatural.Web.Services.Services
{
    public class CustomerService : ICustomerService
    {
        // private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly IOptions<ConfigSettings> _settings;
        public CustomerService(IConfiguration config, IEmailService emailService, IOptions<ConfigSettings> settings, IExigoApiContext exigoApiContext)
        {
            _config = config;
            _emailService = emailService;
            _settings = settings;
            _exigoApiContext = exigoApiContext;
        }

        #region public methods

        public async Task<GetCustomersResponse> GetCustomer(int customerId)
        {
            try
            {
                return await _exigoApiContext.GetContext().GetCustomersAsync(new GetCustomersRequest { CustomerID = customerId });
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
                var aa = await _exigoApiContext.GetContext().GetResourceSetCulturesAsync(req);
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
            string emails = _settings.Value.Emails.ToString();
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
                NoReply = _settings.Value.EmailConfiguration.NoReplyEmail,
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

    }
}
