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
        public CustomerCreateResponse() { }

        public CustomerCreateResponse(CustomerCreateModel user, string token)
        {
            CustomerId = user.CustomerID;
            LoginName = user.LoginName;
            Token = token;
            Email = user.Email;

        }
    }
}
