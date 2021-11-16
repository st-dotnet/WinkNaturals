using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Text;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Setting
{
    public class DistributedCache : ISqlCacheService
    {
        private readonly IDistributedCache _distributedCache;

        public DistributedCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public void DeleteCache(string key)
        {
            _distributedCache.RemoveAsync(key);
        }

        public T GetCacheData<T>(string key)
        {
            var decodedStr = GetCacheData(key);
            return JsonConvert.DeserializeObject<T>(decodedStr);
        }

        public string GetCacheData(string key)
        {
            var data = _distributedCache.Get(key);
            if (data == null) throw new Exception($"Unable to find data for key={key}.");

            var decodedStr = Encoding.UTF8.GetString(data);
            if (string.IsNullOrEmpty(decodedStr)) throw new Exception($"Decoded data is empty for key={key}");

            return decodedStr;
        }

        public void SetCacheData(string key, object value, int expiry)
        {
            if (key == null) throw new Exception($"Unable to define key={key}.");

            _distributedCache.Set(key, (byte[])value, new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(expiry)
            });
        }


    }
}
