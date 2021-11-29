using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Web;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Setting
{
    public class PropertyBags : IPropertyBags
    {
        private static readonly int DbExpire = 1800000;
        private readonly IOptions<ConfigSettings> _config;
        private readonly ISqlCacheService _distributedCache;
        private readonly ICache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PropertyBags(IHttpContextAccessor httpContextAccessor, IOptions<ConfigSettings> config, ISqlCacheService distributedCache, ICache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _distributedCache = distributedCache;
            _cache = cache;
        }
        public void SetCacheSessionData(string sessionID, string data)
        {
            _cache.Set(sessionID, TimeSpan.FromMilliseconds(DbExpire), data);
        }
        public string GetCacheSessionData(string sessionID)
        {
            var sessionData = _cache.Get<string>(sessionID);
            return sessionData;
        }
        public T GetCacheData<T>(string description) where T : IPropertyBag2
        {

            string cookie = _httpContextAccessor.HttpContext.Request.Cookies[description];

            //read cookie from IHttpContextAccessor  
            if (cookie == null)
            {
                Create<T>(description);
            }
            cookie = _httpContextAccessor.HttpContext.Request.Cookies[description];

            string sessionData = "";
            sessionData = GetCacheSessionData(cookie);
            if (string.IsNullOrEmpty(sessionData))
            {
                return Create<T>(description);
            }
            try
            {
                // Deserialize the session data and get our bag.
                dynamic bag = Deserialize<T>(sessionData); // If the customer ID in the bag doesn't match the current customer ID, stop here.
                if (!bag.IsValid())
                {
                    return Create<T>(description);
                } // If we got here, we have a valid property bag. Populate it into the current object.
                return bag;
            }
            catch (Exception ex)
            {
                return Create<T>(description);
            }
        }
        public T Create<T>(string description) where T : IPropertyBag2
        {
            dynamic bag = Activator.CreateInstance(typeof(T));
            bag.SessionID = HttpUtility.UrlEncode(Guid.NewGuid().ToString());
            // bag.SessionID = _httpContextAccessor.HttpContext.Request.Cookies.(Guid.NewGuid().ToString());
            // bag.SessionID= Microsoft.AspNetCore.Http.Extensions.UriHelper.Encode(Guid.NewGuid());
            //bag.SessionID = HttpContext.Current.Server.UrlEncode(Guid.NewGuid().ToString());
            bag.Description = description;
            bag.CreatedDate = DateTime.Now;
            bag = bag.OnBeforeUpdate(bag);
            UpdateCacheData<T>(bag);
            return bag;
        }
        public T UpdateCacheData<T>(T propertyBag) where T : IPropertyBag2
        {
            // Set the session
            SetCacheSessionData(propertyBag.SessionID, Serialize<T>(propertyBag));

            CookieOptions option = new CookieOptions();
            _httpContextAccessor.HttpContext.Response.Cookies.Append("WinkNaturalsReplicatedSiteShoppingCart", propertyBag.Description, option);

            //(propertyBag.Description, propertyBag.SessionID);
            //_httpContextAccessor.HttpContext.  (propertyBag.Description, propertyBag.SessionID); 
            return propertyBag;
        }
        public T Delete<T>(T propertyBag) where T : IPropertyBag2
        {
            var bag = Create<T>(propertyBag.Description);
            return bag;
        }
        public string Serialize<T>(T propertyBag) where T : IPropertyBag2
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            }; return JsonConvert.SerializeObject(propertyBag, settings);
        }
        public T Deserialize<T>(string sessionData) where T : IPropertyBag2
        {

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            return (T)JsonConvert.DeserializeObject(sessionData, typeof(T), settings);
        }
    }
}





