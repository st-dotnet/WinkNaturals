using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Setting.Interfaces;

namespace WinkNaturals.Setting
{
    public class test2 : ICacheProvider
    {

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Purge()
        {
            throw new NotImplementedException();
        }

        public void Set<T>(string key, TimeSpan expiry, T data)
        {
            
        }

        public bool TryGet<T>(string key, out DateTime entryDate, out DateTime now, out T data)
        {
            throw new NotImplementedException();
        }
    }
}
