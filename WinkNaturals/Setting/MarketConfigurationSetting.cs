using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Setting
{
    public class MarketConfigurationSetting : IMarketConfigurationSetting
    {
        private readonly IOptions<ConfigSettings> _config;
        public MarketConfigurationSetting(IOptions<ConfigSettings> config)
        {
            _config = config;
        }
        public Task<Market> GetMarketName(MarketName marketName)
        {

            return (Task<Market>)_config.Value.GlobalMarketSetting.AvailableMarkets.FirstOrDefault(c => c.Name == marketName).GetConfiguration();
        }
    }
}
