using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Setting.Interfaces
{
    public interface ICache
    {
        //T Get<T>(string key, Func<T> command);
        T Get<T>(string key);
        T Get<T>(string key, TimeSpan expiry, Func<T> command);
        void Set<T>(string key, TimeSpan expiry, T data);
    }
}
