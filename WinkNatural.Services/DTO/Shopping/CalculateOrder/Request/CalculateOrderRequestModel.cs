using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNatural.Services.DTO.Shopping
{
    public class CalculateOrderRequestModel
    {
        public int? CustomerID { get; set; }
        public OrderConfiguration Configuration { get; set; }
        public IEnumerable<ShoppingCartItem> Items { get; set; }
        public Address Address { get; set; }
        public int OrderTypeID { get; set; }
        public int ShipMethodID { get; set; }
        public bool ReturnShipMethods { get; set; }
        public decimal? TaxRateOverride { get; set; }
        public Dictionary<string, decimal> ItemPriceOverrides { get; set; }
        public int? PartyID { get; set; }
        public string Other16 { get; set; } // Coupons
        public string Other17 { get; set; } // Points
        public string Other18 { get; set; } // HasSpecialItem
        public string Other20 { get; set; } // Enroll
    }

    public class OrderConfiguration
    {
        public int WarehouseID { get; set; }
        public string CurrencyCode { get; set; }
        public int PriceTypeID { get; set; }
        public int LanguageID { get; set; }
        public string DefaultCountryCode { get; set; }
        public int DefaultShipMethodID { get; set; }
        public int CategoryID { get; set; }
        public int FeaturedCategoryID { get; set; }
        public string Other16 { get; set; } // Coupons
        public string Other17 { get; set; } // Points
        public string Other18 { get; set; } // Has Special Item true or false
        public string Other20 { get; set; } // Enroll
    }

    public class Address
    {
        public AddressType AddressType { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        // public string AddressDisplay { get; }
        public string Error { get; set; }
        public bool IsComplete { get; }
    }

    public class ShippingAddress : Address
    {

        public ShippingAddress() { }
        public ShippingAddress(Address address)
        {
            //AddressType = address.AddressType;
            Address1 = address.Address1;
            Address2 = address.Address2;
            City = address.City;
            State = address.State;
            Zip = address.Zip;
            Country = address.Country;
        }
        public ShippingAddress(Address address, ShippingAddress sAddress)
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
            //Phone = phone;
        }
        public ShippingAddress(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
        // [Required(ErrorMessageResourceName = "FirstNameRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), Display(Name = "FirstName", ResourceType = typeof(Common.Resources.Models))]
        public string FirstName { get; set; }

        // [Display(Name = "MiddleName", ResourceType = typeof(Common.Resources.Models))]
        public string MiddleName { get; set; }

        // [Required(ErrorMessageResourceName = "LastNameRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), Display(Name = "LastName", ResourceType = typeof(Common.Resources.Models))]
        public string LastName { get; set; }

        //[Display(Name = "Company", ResourceType = typeof(Common.Resources.Models))]
        public string Company { get; set; }

        // [Required(ErrorMessageResourceName = "PhoneNumberRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), DataType(DataType.PhoneNumber), Display(Name = "PhoneNumber", ResourceType = typeof(Common.Resources.Models))]
        //[RegularExpression(GlobalSettings.RegularExpressions.PhoneNumber, ErrorMessageResourceName = "IncorrectPhone", ErrorMessageResourceType = typeof(Common.Resources.Models))]
        public string Phone { get; set; }

        // [Required(ErrorMessageResourceName = "EmailRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), DataType(DataType.EmailAddress), RegularExpression(GlobalSettings.RegularExpressions.EmailAddresses, ErrorMessageResourceName = "IncorrectEmail", ErrorMessageResourceType = typeof(Common.Resources.Models)), Display(Name = "Email", ResourceType = typeof(Common.Resources.Models))]
        public string Email { get; set; }

        public string FullName
        {
            get { return string.Join(" ", this.FirstName, this.LastName); }
        }

        public string AddressDisplay { get; }
        //public AddressType AddressType { get; set; }

        ////[Required(ErrorMessageResourceName = "AddressOneRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), Display(Name = "AddressOne", ResourceType = typeof(Common.Resources.Models))]
        //public string Address1 { get; set; }

        ////[Display(Name = "AddressTwo", ResourceType = typeof(Common.Resources.Models))]
        //public string Address2 { get; set; }

        ////[Required(ErrorMessageResourceName = "CityRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), Display(Name = "City", ResourceType = typeof(Common.Resources.Models))]
        //public string City { get; set; }

        ////[Required(ErrorMessageResourceName = "StateRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), Display(Name = "State", ResourceType = typeof(Common.Resources.Models))]
        //public string State { get; set; }

        ////[Required(ErrorMessageResourceName = "ZipRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), Display(Name = "Zip", ResourceType = typeof(Common.Resources.Models))]
        //public string Zip { get; set; }

        ////[Required(ErrorMessageResourceName = "CountryRequired", ErrorMessageResourceType = typeof(Common.Resources.Models)), Display(Name = "Country", ResourceType = typeof(Common.Resources.Models))]
        //public string Country { get; set; }

    }
    public interface IAddress
    {
        AddressType AddressType { get; set; }


        string Address1 { get; set; }

        string Address2 { get; set; }

        string City { get; set; }

        string State { get; set; }

        string Zip { get; set; }

        string Country { get; set; }
    }

    public class ShoppingCartItem
    {

        public Guid ID { get; set; }
        public string ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public string ParentItemCode { get; set; }
        public string GroupMasterItemCode { get; set; }
        public string DynamicKitCategory { get; set; }
        public ShoppingCartItemType Type { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public bool OtherCheck2 { get; set; }
        public Nullable<decimal> PriceEachOverride { get; set; }
        public Nullable<decimal> TaxableEachOverride { get; set; }
        public Nullable<decimal> BusinessVolumeEachOverride { get; set; }
        public Nullable<decimal> CommissionableVolumeEachOverride { get; set; }
        public Nullable<decimal> ShippingPriceEachOverride { get; set; }
    }
    public enum AddressType
    {
        New = 0,
        Main = 1,
        Mailing = 2,
        Other = 3
    }
    public enum ShoppingCartItemType
    {
        Order = 0,
        AutoOrder = 1,
        WishList = 2,
        Coupon = 3,
        EnrollmentPack = 4,
        EnrollmentAutoOrderPack = 5
    }
    public static class OrderTypes
    {
        /// <summary>
        ///	Order Type 1
        /// </summary>
        public const int CustomerService = 1;
        /// <summary>
        ///	Order Type 2
        /// </summary>
        public const int ShoppingCart = 2;
        /// <summary>
        ///	Order Type 3
        /// </summary>
        public const int WebWizard = 3;
        /// <summary>
        ///	Order Type 4
        /// </summary>
        public const int RecurringOrder = 4;
        /// <summary>
        ///	Order Type 5
        /// </summary>
        public const int Import = 5;
        /// <summary>
        ///	Order Type 6
        /// </summary>
        public const int BackOrder = 6;
        /// <summary>
        ///	Order Type 7
        /// </summary>
        public const int ReplacementOrder = 7;
        /// <summary>
        ///	Order Type 8
        /// </summary>
        public const int ReturnOrder = 8;
        /// <summary>
        ///	Order Type 9
        /// </summary>
        public const int WebRecurringOrder = 9;
        /// <summary>
        ///	Order Type 10
        /// </summary>
        public const int TicketSystem = 10;
        /// <summary>
        ///	Order Type 11
        /// </summary>
        public const int APIOrder = 11;
        /// <summary>
        ///	Order Type 12
        /// </summary>
        public const int BackOrderParentNoShip = 12;
        /// <summary>
        ///	Order Type 13
        /// </summary>
        public const int ChildOrder = 13;
    }
}
