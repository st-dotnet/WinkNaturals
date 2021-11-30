using Exigo.Api.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.DTO.Shopping;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface IEnrollmentService
    {
        //Get packs data
        List<EnrollmentResponse> GetItems();


        //Process exigo payment
        Task<TransactionalResponse> SubmitCheckout(TransactionalRequestModel transactionRequest, int customerId);

      //  string GetDistributors(string query);

    }
}
