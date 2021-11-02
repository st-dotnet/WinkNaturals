using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Models
{
    public class Market : IMarket
    {
        public Market()
        {
            this.Configuration = GetConfiguration();
        }

        public MarketName Name { get; set; }
        public string Description { get; set; }
        public string CookieValue { get; set; }
        public string CultureCode { get; set; }
        public bool IsDefault { get; set; }
        public IEnumerable<string> Countries { get; set; }

        //public List<IPaymentMethod> AvailablePaymentTypes { get; set; }
        //public List<Language> AvailableLanguages { get; set; }
        //public List<Common.Api.ExigoWebService.FrequencyType> AvailableAutoOrderFrequencyTypes { get; set; }
        public List<int> AvailableShipMethods { get; set; }
        public IMarketConfiguration Configuration { get; set; }
        public virtual IMarketConfiguration GetConfiguration()
        {
            return new UnitedStatesConfiguration();

        }
    }
}