using System.Collections.Generic;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Utilities;

namespace WinkNaturals.Models
{
    public class UnitedStatesConfiguration : IMarketConfiguration
    {
        private MarketName marketName = MarketName.UnitedStates;

        public MarketName MarketName => MarketName.UnitedStates;

        #region Properties
        // Shopping
        public IOrderConfiguration Orders
        {
            get
            {
                return new OrderConfiguration();
            }
        }


        // Enrollment Packs
        public IOrderConfiguration EnrollmentKits
        {
            get
            {
                return new EnrollmentKitConfiguration();
            }
        }
        #endregion

        // Base Order Configuration
        public class BaseOrderConfiguration : IOrderConfiguration
        {
            public BaseOrderConfiguration()
            {
                WarehouseID = Warehouses.Default;
                CurrencyCode = CurrencyCodes.DollarsUS;
                PriceTypeID = PriceTypes.Retail;
                LanguageID = Languages.English;
                DefaultCountryCode = "US";
                DefaultShipMethodID = 6;
                AvailableShipMethods = new List<int> { 6, 7 };
            }


            public int WarehouseID { get; set; }
            public string CurrencyCode { get; set; }
            public int PriceTypeID { get; set; }
            public int LanguageID { get; set; }
            public string DefaultCountryCode { get; set; }
            public int DefaultShipMethodID { get; set; }
            public List<int> AvailableShipMethods { get; set; }
            public int CategoryID { get; set; }
            public int FeaturedCategoryID { get; set; }
            public string Other16 { get; set; } // Coupons
            public string Other17 { get; set; } // Points
            public string Other18 { get; set; } // Has Special Item  true or false
            public string Other20 { get; set; } // Enroll
        }

        #region Configurations
        // Replicated Site - Product List
        public class OrderConfiguration : BaseOrderConfiguration
        {
            public OrderConfiguration()
            {
                CategoryID = 3;
            }
        }

        // Replicated Site - Auto Order Manager
        //public class AutoOrderConfiguration : BaseOrderConfiguration
        //{
        //    public AutoOrderConfiguration()
        //    {
        //        CategoryID = 4;
        //        PriceTypeID = PriceTypes.Preferred;
        //        DefaultShipMethodID = 8;
        //    }
        //}



        // Replicated Site - Enrollment Kits
        public class EnrollmentKitConfiguration : BaseOrderConfiguration
        {
            public EnrollmentKitConfiguration()
            {
                CategoryID = 20;
                DefaultShipMethodID = 8;
            }
        }




        #endregion
    }
}