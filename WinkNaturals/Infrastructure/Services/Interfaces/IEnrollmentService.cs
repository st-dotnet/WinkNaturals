﻿using Exigo.Api.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.DTO.Customer;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Infrastructure.Services;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using CreditCard = WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.CreditCard;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface IEnrollmentService
    {
        //Get packs data
       List<EnrollmentResponse> GetItems();
        //Process exigo payment
      //  Task<TransactionalResponse> SubmitCheckout(TransactionalRequestModel transactionRequest, int customerId);
        Task<TransactionalResponse> SubmitCheckout(TransactionalRequestModel transactionRequest);
        List<dynamic> GetDistributors(int customerId);
        Task<CreditCard> SetCustomerCreditCard(int customerID, CreditCard card);
    }
}
