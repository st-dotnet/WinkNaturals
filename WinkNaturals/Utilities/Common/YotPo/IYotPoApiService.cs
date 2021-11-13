using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Utilities.Api.ThirdParty;
using WinkNaturals.Utilities.Common.YotPo.Domain;

namespace WinkNaturals.Utilities.Common.YotPo
{
    public interface IYotPoApiService
    {
        AuthResponse GetAuthToken();

        ServiceResponse PostOrder(PurchaseRequest purchase);
    }
}
