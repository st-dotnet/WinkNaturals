using System.Collections.Generic;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IMarket
    {
        MarketName Name { get; set; }
        string Description { get; set; }
        string CookieValue { get; set; }
        string CultureCode { get; set; }
        bool IsDefault { get; set; }
        IEnumerable<string> Countries { get; set; }

        IMarketConfiguration Configuration { get; set; }
    }

    public enum MarketName
    {
        UnitedStates,
        Canada
    }
}
