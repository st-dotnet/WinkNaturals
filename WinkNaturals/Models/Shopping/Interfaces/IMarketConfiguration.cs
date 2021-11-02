using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IMarketConfiguration
    {
        MarketName MarketName { get; }

        IOrderConfiguration Orders { get; }

    }
}
