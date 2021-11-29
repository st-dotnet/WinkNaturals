using Exigo.Api.Client;
using ExigoAPIRef;
using Microsoft.Extensions.Options;
using System;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Setting
{
    public class ExigoApiContext : IExigoApiContext
    {
        private readonly IOptions<ConfigSettings> _config;


        public ExigoApiContext(IOptions<ConfigSettings> config)
        {
            _config = config;
        }
        public ExigoApiClient GetContext(bool isSandbox = false)
        {
            if (isSandbox == false)
            {
                return new ExigoApiClient(new Uri("https://winknaturals-api.exigo.com/3.0/ExigoApi.asmx"), _config.Value.ExigoConfig.CompanyKey, _config.Value.ExigoConfig.LoginName, _config.Value.ExigoConfig.Password);
            }
            else
            {
                return new ExigoApiClient(new Uri("https://sandboxapi2.exigo.com/3.0/ExigoApi.asmx"), _config.Value.ExigoConfig.CompanyKey, _config.Value.ExigoConfig.LoginName, _config.Value.ExigoConfig.Password);
            }
            // return new ExigoApiClient(_config.Value.ExigoConfig.CompanyKey, _config.Value.ExigoConfig.LoginName, _config.Value.ExigoConfig.Password);
            //  return _apiAuthentication;
        }

     
    }
}