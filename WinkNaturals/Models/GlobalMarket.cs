using Microsoft.Extensions.Options;
using System;
using System.Linq;
using WinkNaturals.Setting;

namespace WinkNaturals.Models
{
    public class GlobalMarket
    {
        private readonly IOptions<ConfigSettings> _config;

        public GlobalMarket(IOptions<ConfigSettings> config)
        {
            _config = config;
        }
        public Market GetCurrentMarket(string CountryCode)
        {
            // Get the user's country to see which market we are in
            var country = CountryCode; //GlobalUtilities.GetSelectedCountryCode();

            if (string.IsNullOrEmpty(country))
            {
                country = _config.Value.GlobalMarketSetting.AvailableMarkets.Where(c => c.IsDefault == true).FirstOrDefault().Countries.FirstOrDefault();
                // country = GlobalSettings.Markets.AvailableMarkets.Where(c => c.IsDefault == true).FirstOrDefault().Countries.FirstOrDefault();
            }

            // If the country cookie in null or empty then create it
            //var countryCookie = Common.GlobalUtilities.SetSelectedCountryCode(country);

            var market = _config.Value.GlobalMarketSetting.AvailableMarkets.Where(c => c.Countries.Contains(country)).FirstOrDefault();

            // If we didn't find a market for the user's country, get the first default market
            if (market == null) market = _config.Value.GlobalMarketSetting.AvailableMarkets.Where(c => c.IsDefault == true).FirstOrDefault();

            // If we didn't find a default market, get the first market we find
            if (market == null) market = _config.Value.GlobalMarketSetting.AvailableMarkets.FirstOrDefault();

            // Return the market
            return market;
        }
    }
}
