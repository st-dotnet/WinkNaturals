using System.Collections.Generic;

namespace WinkNaturals.Models
{
    public class UnitedStatesMarket : Market
    {
        public UnitedStatesMarket()
            : base()
        {
            Name = (Shopping.Interfaces.MarketName)MarketName.UnitedStates;
            Description = "United States";
            CookieValue = "US";
            CultureCode = "en-US";
            IsDefault = true;
            Countries = new List<string> { "US" };

            //AvailablePaymentTypes = new List<IPaymentMethod>
            //{
            //    new CreditCard()
            //};

            //AvailableLanguages = new List<Language>
            //{
            //    new Language(Languages.English, "en-US")
            //};

            //AvailableAutoOrderFrequencyTypes = new List<Api.ExigoWebService.FrequencyType>
            //{
            //    Api.ExigoWebService.FrequencyType.Monthly,
            //    Api.ExigoWebService.FrequencyType.EveryEightWeeks,
            //    Api.ExigoWebService.FrequencyType.Quarterly
            //};

            // AvailableShipMethods = new List<int> { 6, 7 };
        }

        //public override IMarketConfiguration GetConfiguration()
        //{
        //    return new UnitedStatesConfiguration();
        //}

        public enum MarketName
        {
            UnitedStates,
            Canada
        }
    }
}
