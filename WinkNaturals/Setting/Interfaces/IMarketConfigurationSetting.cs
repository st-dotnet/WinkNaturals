using System.Threading.Tasks;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Setting.Interfaces
{
    public interface IMarketConfigurationSetting
    {
        Task<Market> GetMarketName(MarketName marketName);
    }
}
