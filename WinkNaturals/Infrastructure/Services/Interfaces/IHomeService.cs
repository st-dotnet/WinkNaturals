using Exigo.Api.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Customer; 
using WinkNaturals.Models;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface IHomeService
    {
        //Send email from Exigo service
        Task<ContactResponse> SendEmail(ContactRequest request);

        //Get home page reviews
        List<HomePageReviewsModel> GetReviews();

    }
}
