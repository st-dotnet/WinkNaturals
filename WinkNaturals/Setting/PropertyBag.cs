using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Web;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Setting
{
    public class PropertyBag : IPropertyBag2
    {
        private static readonly int DbExpire = 1800000;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<ConfigSettings> _config;
        private readonly object _distributedCache;

        // private readonly ISqlCacheService _distributedCache;

        private readonly ICache _cache;

        public PropertyBag(IHttpContextAccessor httpContextAccessor, IOptions<ConfigSettings> config, ICache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _cache = cache;
        }


        public string Version { get; set; }
        public string Description { get; set; }
        public string SessionID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Expires { get; set; }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        public T OnBeforeUpdate<T>(T propertyBag) where T : IPropertyBag2
        {
            throw new NotImplementedException();
        }

        public string GetCacheSessionData(string sessionID)
        {
            var sessionData = _cache.Get<string>(sessionID);
            return sessionData;
        }
        public void SetCacheSessionData(string sessionID, string data)
        {
            // _distributedCache.SetStringAsync(sessionID, data);
            _cache.Set(sessionID, TimeSpan.FromMilliseconds(DbExpire), data);
        }
        public T GetCacheData<T>(string description) where T : IPropertyBag2
        {
            string cookie = _httpContextAccessor.HttpContext.Request.Cookies[description];
            if (cookie == null)
            {
                return Create<T>(description);
            }
            string sessionData = "";
            sessionData = GetCacheSessionData(cookie);

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
        public T Create<T>(string description) where T : IPropertyBag2
        {
            dynamic bag = Activator.CreateInstance(typeof(T));

            bag.SessionID = HttpUtility.UrlEncode(Guid.NewGuid().ToString());
            bag.Description = description;
            bag.CreatedDate = DateTime.Now;
            bag = bag.OnBeforeUpdate(bag);

            UpdateCacheData<T>(bag);

            return bag;
        }
        public string Serialize<T>(T propertyBag) where T : IPropertyBag2
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            return JsonConvert.SerializeObject(propertyBag, settings);
        }
        public T Deserialize<T>(string sessionData) where T : IPropertyBag2
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            return (T)JsonConvert.DeserializeObject(sessionData, typeof(T), settings);
        }

        public T UpdateCacheData<T>(T propertyBag) where T : IPropertyBag2
        {

            // Set the session
            SetCacheSessionData(propertyBag.SessionID, Serialize<T>(propertyBag));

            // Set the cookie
            //var cookie = new HttpCookie(propertyBag.Description, propertyBag.SessionID);
            //if (propertyBag.Expires > 0)
            //{
            //    cookie.Expires = DateTime.Now.AddMinutes(propertyBag.Expires);
            //}
            //_httpContextAccessor.HttpContext.Response.Cookies.Append(cookie);

            return propertyBag;
        }


        //public T UpdateCacheData<T>(T propertyBag) where T : IPropertyBag
        //{

        //    // Set the session
        //    SetCacheSessionData(propertyBag.SessionID, Serialize<T>(propertyBag));

        //    // Set the cookie
        //    //var cookie = new HttpCookie(propertyBag.Description, propertyBag.SessionID);
        //    //if (propertyBag.Expires > 0)
        //    //{
        //    //    cookie.Expires = DateTime.Now.AddMinutes(propertyBag.Expires);
        //    //}
        //    //_httpContextAccessor.HttpContext.Response.Cookies.Append(cookie);

        //    return propertyBag;
        //}


    }
}
