using System;
using System.Net.Security;
using ExigoAPIRef;
using WinkNatural.Web.Common;
using WinkNatural.Web.Common.Utils.Enum;
using WinkNaturals.Models;

namespace WinkNatural.Web.Services.DTO.Customer
{
    public class CustomerCreateResponse
    {
        public int CustomerId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string LoginName { get; set; }
        public string Token { get; set; }
        public string ErrorMessage { get; set; }
        public string TypeOfCustomer { get; set; }
        
        public CustomerCreateResponse() { }
        public CustomerCreateResponse(CustomerCreateModel user, string token)
        {
            CustomerId = user.CustomerID;
            LoginName = user.LoginName;
            Token = token;
            Email = user.Email;
            TypeOfCustomer = user.CustomerType;

        }
    }
}
