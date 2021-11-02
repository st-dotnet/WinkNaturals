namespace WinkNatural.Common.Utils.Enum
{
    public enum CustomerTypeEnum
    {
        /// <summary>
        ///	Customer Type 1
        /// </summary>
        RetailCustomer = 1,
        /// <summary>
        ///	Customer Type 2
        /// </summary>
        PreferredCustomer = 2,
        /// <summary>
        ///	Customer Type 3
        /// </summary>
        Distributor = 3,
        /// <summary>
        ///	Customer Type 3
        /// </summary>
        Distributor2 = 4,
        /// <summary>
        ///	Customer Type 3
        /// </summary>
        Distributor3 = 5
    }

    public enum CustomerStatuses
    {
        /// <summary>
        ///	Customer Status 0
        /// </summary>
        Deleted = 0,
        /// <summary>
        ///	Customer Status 1
        /// </summary>
        Active = 1
    }

    public enum ItemInventory
    {
        AVAILABLE = 1,
        OUTOFSTOCK = 2,
        PRESALE = 3
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
}
