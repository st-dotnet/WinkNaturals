using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Setting
{
    public class test : ICache
    {
        private readonly ICacheProvider _cacheProvider;

        public test(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }
        public T Get<T>(string key)
        {
            if (_cacheProvider == null)
                throw new Exception("DataCacheProvider is not set. Run DataCacheConfig.Initialize in service.");

            DateTime entryDate;
            DateTime serverDate;

            T result;
            // Try to get the data
            _cacheProvider.TryGet<T>(key, out entryDate, out serverDate, out result);

            return result;
        }

        public T Get<T>(string key, TimeSpan expiry, Func<T> command)
        {
            return Get(key, expiry, command);
        }

        public void Set<T>(string key, TimeSpan expiry, T data)
        {
            Set<T>(key, expiry, data);
        }
    }
}
