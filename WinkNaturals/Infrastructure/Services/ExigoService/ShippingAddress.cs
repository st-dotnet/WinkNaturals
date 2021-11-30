using System.ComponentModel.DataAnnotations;

namespace WinkNaturals.Infrastructure.Services.ExigoService
{
    public class ShippingAddress : Address
    {
        public ShippingAddress() { }
        public ShippingAddress(Address address)
        {
            AddressType = address.AddressType;
            Address1 = address.Address1;
            Address2 = address.Address2;
            City = address.City;
            State = address.State;
            Zip = address.Zip;
            Country = address.Country;
        }
        public ShippingAddress(Address address, ShippingAddress sAddress, string phone)
        {
            AddressType = address.AddressType;
            Address1 = address.Address1;
            Address2 = address.Address2;
            City = address.City;
            State = address.State;
            Zip = address.Zip;
            Country = address.Country;
            FirstName = sAddress.FirstName;
            LastName = sAddress.LastName;
            Phone = phone;
        }
        public ShippingAddress(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        [Required(ErrorMessageResourceName = "FirstNameRequired"), Display(Name = "FirstName")]
        public string FirstName { get; set; }

        [Display(Name = "MiddleName")]
        public string MiddleName { get; set; }

        [Required(ErrorMessageResourceName = "LastNameRequired"), Display(Name = "LastName")]
        public string LastName { get; set; }

        [Display(Name = "Company")]
        public string Company { get; set; }

        [Required(ErrorMessageResourceName = "PhoneNumberRequired"), DataType(DataType.PhoneNumber), Display(Name = "PhoneNumber")]

        public string Phone { get; set; }

        [Required(ErrorMessageResourceName = "EmailRequired"), DataType(DataType.EmailAddress), Display(Name = "Email")]
        public string Email { get; set; }

        public string FullName
        {
            get { return string.Join(" ", this.FirstName, this.LastName); }
        }
    }
}