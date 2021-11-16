using Exigo.Api.Client;
using Microsoft.Extensions.Options;
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
        public ExigoApiClient GetContext()
        {
            return new ExigoApiClient(_config.Value.ExigoConfig.CompanyKey, _config.Value.ExigoConfig.LoginName, _config.Value.ExigoConfig.Password);
        }
    }
}