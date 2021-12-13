using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models;

namespace WinkNaturals.Infrastructure.Services.Interfaces
{
    public interface IAccountService
    {
        IEnumerable<PointTransaction> LoyaltyPointsService(int customerId,int LoyaltyPointAccountId);
    }
}
