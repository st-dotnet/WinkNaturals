using Microsoft.Extensions.Options;
using System;
using System.Linq;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Models
{
    public class GetCurrentMarket : IGetCurrentMarket
    {
        private readonly IOptions<ConfigSettings> _config;

        public GetCurrentMarket(IOptions<ConfigSettings> config)
        {
            _config = config;
        }

        public Market curretMarket(string country)
        {

            var countyObject = country; //GlobalUtilities.GetSelectedCountryCode();

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

            return market;
        }
    }
}

