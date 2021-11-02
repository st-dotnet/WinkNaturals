using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models;

namespace WinkNaturals.Setting.Interfaces
{
    public interface IGetCurrentMarket
    {
       Market curretMarket(string country);
    }
}
