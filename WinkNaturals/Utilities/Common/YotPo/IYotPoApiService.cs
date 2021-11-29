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
