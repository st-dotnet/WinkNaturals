using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Setting.Interfaces
{
    public  interface IExigoApiContext
    {
        public ExigoApiClient GetContext();
    }
}
