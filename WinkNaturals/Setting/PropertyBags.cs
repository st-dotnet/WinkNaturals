using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Setting
{
    public class PropertyBags
    {
        private static readonly int DbExpire = 1800000;
        private readonly IOptions<ConfigSettings> _config;
        // private readonly ISqlCacheService _distributedCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ICache _cache;
        private static IHttpContextAccessor _httpContextAccessor;

        public PropertyBags(IHttpContextAccessor httpContextAccessor, IOptions<ConfigSettings> config, IDistributedCache distributedCache, ICache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _distributedCache = distributedCache;
            _cache = cache;
        }
        public string Version { get; set; }
        public string Description { get; set; }
        public string SessionID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Expires { get; set; }

        public T Get<T>(string description) where T : IPropertyBag
        {
            // Attempt to load the bag from the cookie

            var cookie = HttpContext.Current.Request.Cookies[description];
            if (cookie == null)
            {
                return Create<T>(description);
            }

            string sessionData = "";

            sessionData = GetSessionData(cookie.Value);

            if (string.IsNullOrEmpty(sessionData))
            {
                return Create<T>(description);
            }

            try
            {
                // Deserialize the session data and get our bag.
                dynamic bag = Deserialize<T>(sessionData);

                // If the customer ID in the bag doesn't match the current customer ID, stop here.
                if (!bag.IsValid())
                {
                    return Create<T>(description);
                }

                // If we got here, we have a valid property bag. Populate it into the current object.
                return bag;
            }
            catch
            {
                return Create<T>(description);
            }
        }

        //public string GetCacheSessionData(string sessionID)
        //{
        //    var sessionData = _cache.Get<string>(sessionID);
        //    // string sessionData = _distributedCache.GetAsync(sessionID);
        //    return sessionData;
        //}
        //public void SetCacheSessionData(string sessionID, string data)
        //{
        //    // _distributedCache.SetStringAsync(sessionID, data);
        //    _cache.Set(sessionID, TimeSpan.FromMilliseconds(DbExpire), data);
        //}

        //public T GetCacheData<T>(string description) where T : IPropertyBag
        //{
        //    string cookie = _httpContextAccessor.HttpContext.Request.Cookies[description];
        //    if (cookie == null)
        //    {
        //        return Create<T>(description);
        //    }
        //    string sessionData = "";
        //    sessionData = GetCacheSessionData(cookie);

        //    if (string.IsNullOrEmpty(sessionData))
        //    {
        //        return Create<T>(description);
        //    }

        //    try
        //    {
        //        // Deserialize the session data and get our bag.
        //        dynamic bag = Deserialize<T>(sessionData);

        //        // If the customer ID in the bag doesn't match the current customer ID, stop here.
        //        if (!bag.IsValid())
        //        {
        //            return Create<T>(description);
        //        }

        //        // If we got here, we have a valid property bag. Populate it into the current object.
        //        return bag;
        //    }
        //    catch
        //    {
        //        return Create<T>(description);
        //    }
        //    public T Create<T>(string description) where T : IPropertyBag
        //    {
        //        dynamic bag = Activator.CreateInstance(typeof(T));

        //        bag.SessionID = HttpUtility.UrlEncode(Guid.NewGuid().ToString());
        //        bag.Description = description;
        //        bag.CreatedDate = DateTime.Now;
        //        bag = bag.OnBeforeUpdate(bag);

        //        UpdateCacheData<T>(bag);

        //        return bag;
        //    }
        //    public T UpdateCacheData<T>(T propertyBag) where T : IPropertyBag
        //    {

        //        // Set the session
        //        SetCacheSessionData(propertyBag.SessionID, Serialize<T>(propertyBag));

        //        // Set the cookie
        //        //var cookie = new HttpCookie(propertyBag.Description, propertyBag.SessionID);
        //        //if (propertyBag.Expires > 0)
        //        //{
        //        //    cookie.Expires = DateTime.Now.AddMinutes(propertyBag.Expires);
        //        //}
        //        //_httpContextAccessor.HttpContext.Response.Cookies.Append(cookie);

        //        return propertyBag;
        //    }
    }
}

