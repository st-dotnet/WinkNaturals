using WinkNaturals.Models;

namespace WinkNaturals.Setting.Interfaces
{
    public interface IGetCurrentMarket
    {
        Market curretMarket(string country);
    }
}
