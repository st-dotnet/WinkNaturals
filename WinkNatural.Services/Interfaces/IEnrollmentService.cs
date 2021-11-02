using Exigo.Api.Client;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;

namespace WinkNatural.Services.Interfaces
{
    public interface IEnrollmentService
    {
        //Get packs data
        List<EnrollmentResponse> GetItems();

       
        //Process exigo payment
        Task<TransactionalResponse> ProcessExigoPayment(); //TransactionalRequest request
    }
}
