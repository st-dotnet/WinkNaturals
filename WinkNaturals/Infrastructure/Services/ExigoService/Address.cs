using System;
using System.ComponentModel.DataAnnotations;
using WinkNaturals.Helpers;

namespace WinkNaturals.Infrastructure.Services.ExigoService
{
    public class Address : IAddress
    {
        public Address()
        {
            AddressType = AddressType.New;
        }
        public Address(string country, string state) : this()
        {
            Country = country;
            State = state;
        }

        public Address(ShippingAddress shippingAddress)
        {
            AddressType = shippingAddress.AddressType;
            Address1 = shippingAddress.Address1;
            Address2 = shippingAddress.Address2;
            City = shippingAddress.City;
            State = shippingAddress.State;
            Zip = shippingAddress.Zip;
            Country = shippingAddress.Country;
        }

        [Required]
        public AddressType AddressType { get; set; }

        [Required(ErrorMessageResourceName = "AddressOneRequired"), Display(Name = "AddressOne")]
        public string Address1 { get; set; }

        [Display(Name = "AddressTwo")]
        public string Address2 { get; set; }

        [Required(ErrorMessageResourceName = "CityRequired"), Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessageResourceName = "StateRequired"), Display(Name = "State")]
        public string State { get; set; }

        [Required(ErrorMessageResourceName = "ZipRequired"), Display(Name = "Zip")]
        public string Zip { get; set; }

        [Required(ErrorMessageResourceName = "CountryRequired"), Display(Name = "Country")]
        public string Country { get; set; }

        public string AddressDisplay
        {
            get { return this.Address1 + ((String.IsNullOrEmpty(Address2)) ? " {0}" + (Address2) : ""); }
        }
        public bool IsComplete
        {
            get
            {
                return
                    !string.IsNullOrEmpty(Address1) &&
                    !string.IsNullOrEmpty(City) &&
                    !string.IsNullOrEmpty(State) &&
                    !string.IsNullOrEmpty(Zip) &&
                    !string.IsNullOrEmpty(Country);
            }
        }

        public string GetHash()
        {

            return Security.GetHashString(string.Format("{0}|{1}|{2}|{3}|{4}",
               this.AddressDisplay.Trim(),
               this.City.Trim(),
               this.State.Trim(),
               this.Zip.Trim(),
               this.Country.Trim()));
        }

        public override string ToString()
        {
            return this.Address1 + " " + (String.IsNullOrEmpty(Address2) ? this.Address2 + " " : string.Empty) + this.City + " " + this.State + " " + this.Zip;
        }
        public override bool Equals(object obj)
        {
            try
            {
                var hasha = this.GetHash();
                var hashb = ((Address)obj).GetHash();
                return hasha.Equals(hashb);
            }
            catch
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static implicit operator Address(WinkNatural.Web.Services.DTO.Shopping.Address v)
        {
            throw new NotImplementedException();
        }
    }
}