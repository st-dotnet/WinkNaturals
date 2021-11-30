using System;

namespace WinkNaturals.Setting.Interfaces
{
    public interface ICacheProvider
    {
        void Initialize();
        void Purge();
        bool TryGet<T>(string key, out DateTime entryDate, out DateTime now, out T data);
        void Set<T>(string key, TimeSpan expiry, T data);
    }
}
