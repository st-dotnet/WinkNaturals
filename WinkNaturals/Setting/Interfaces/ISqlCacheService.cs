using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Setting.Interfaces
{
        public interface ISqlCacheService
        {
            T GetCacheData<T>(string key);
            string GetCacheData(string key);
            void SetCacheData(string key,  object value, int expiry);
            void DeleteCache(string key);
    }
}
