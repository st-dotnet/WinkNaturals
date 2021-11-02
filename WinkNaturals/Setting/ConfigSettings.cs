using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Setting
{
    public class ConnectionStrings
    {
        public string DefaultConnection { get; set; }
        public string DefaultConnectionforCache { get; set; }
    }
    public class ConfigSettings
    {
        public string Title { get; set; }
        public ReplicatedSites ReplicatedSites { get; set; }

        public MailChimp MailChimp { get; set; }
        public YotPo YotPo { get; set; }

        public BraintreeConfiguration BraintreeConfiguration { get; set; }

        public int Items { get; set; }

        public MarketsSetting Markets { get; set; }

        public Globalization Globalization { get; set; }
        public Company Company { get; set; }
        public GlobalItems GlobalItems { get; set; }
        public Api Api { get; set; }
        public ExigoConfigSetting ExigoConfig { get; set; }
        public GlobalMarketSetting GlobalMarketSetting { get; set; }
        public EmailConfiguration EmailConfiguration { get; set; }
        public Emails Emails { get; set; }
        public JwtSettings JwtSettings { get; set; }

        public AppSettings AppSettings { get; set; }


    }
    public class AppSettings
    {
        public string billerAccountId { get; set; }
        public string authToken { get; set; }
        public string APIKey { get; set; }
        public string TransactionKey { get; set; }

    } 
    public class JwtSettings
    {
        public string Key { get; set; }
    }
    public class Emails
    {
        public string VerifyEmailUrl { get; set; }
    }
    public class ConfigurationManager
    {
        public string AppSettings { get; set; }

    }
    public class ExigoConfigSetting
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string CompanyKey { get; set; }
        public string SandboxID { get; set; }
    }
    public class Api
    {
        public int CacheTimeout { get; set; }
        public  string LoginName  { get; set; }
        public  string Password  { get; set; }
        //Errors out without exception and won't authenticate when CompanyKey is incorrect
        public  string CompanyKey  { get; set; }
    }
    public class GlobalItems
    {
        public int WebID { get; set; }
    }
    public class Company
    {
        public string BaseReplicatedUrl { get; set; }
      
       
    }

    public class ReplicatedSites
    {
        public string SessionTimeout { get; set; }
        public int DefaultWebAlias { get; set; }
        public int DefaultAccountID { get; set; }
        public int IdentityRefreshInterval { get; set; }

        public string FormattedBaseUrl { get; set; }
        public string GoogleAnalyticsWebPropertyID { get; set; }
        public bool SkipJoinEnroller { get; set; }
        public bool ShowEnrollerChoiceAtShop { get; set; }
    }
    public class MailChimp
    {
        public string ApiBaseUrl { get; set; }
        public string ApiKey { get; set; }
    }
    public class YotPo
    {
        public  string APIUrl { get; set; }
        public  string ApiKey { get; set; }
        public  string ApiSecrete { get; set; }
        public  string Platform { get; set; }
        public string HomePageEndpoints { get; set; }
    }
    public class BraintreeConfiguration
    {
        public string Environment { get; set; }
        public string MerchantId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }

    public class MarketsSetting
    {
        public string MarketCookieName { get; set; }
    }
    public class Globalization
    {
        public  string CookieKey { get; set; }
        public  string CountryCookieName { get; set; }
        public  string CountryCookieChosenName { get; set; }
        public  string SiteCultureCookieName { get; set; }
        public  string LanguageCookieName { get; set; }
        public  string LogInAlertCookieName { get; set; }
        public static string BaseImageURL { get; set; }
    }
    public class GlobalMarketSetting
    {
        //JS, 09/11/2015
        //Removed the Comma because Commas will break Cookie Names in Safari
         public string MarketCookieName { get; set; }

        public List<Market> AvailableMarkets = new List<Market>
            {
                new UnitedStatesMarket()
            };
    }
   
    public class EmailConfiguration
    {
        public string NoReplyEmail { get; set; }
        public string CompanyName { get; set; }
        public string ContactUsEmail { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

}

