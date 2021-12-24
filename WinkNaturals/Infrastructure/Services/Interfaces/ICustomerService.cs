﻿using Exigo.Api.Client;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.DTO;
using static WinkNaturals.Helpers.Constant;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface ICustomerService
    {
        //Get customer
        Task<GetCustomersResponse> GetCustomer(int customerId);
        Task<string> GetImage();

        Task<UpdateCustomerResponse> UpdateCustomer(UpdateCustomerRequest request);

        Task<bool> SendEmailVerification(int customerId, string email);

        Task<SetAccountResponse> DeleteCustomerCreditCard(int customerID, CreditCardType type);
        Task DeleteCustomerAutoOrder(int customerID, int autoOrderID);

        

        
    }
}
