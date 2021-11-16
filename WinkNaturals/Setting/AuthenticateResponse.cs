using WinkNaturals.Models;

namespace WinkNaturals.Setting
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }


        public AuthenticateResponse(CustomerCreateModel user, string token)
        {
            Id = user.CustomerID;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.LoginName;
            Email = user.Email;
            Token = token;
        }
    }
}
