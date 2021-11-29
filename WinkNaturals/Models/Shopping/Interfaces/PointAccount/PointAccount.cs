using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;

namespace WinkNaturals.Models.Shopping.Interfaces.PointAccount
{
    public class PointAccount : IPointAccount
    {
        public int PointAccountID { get; set; }
        public string PointAccountDescription { get; set; }
    }
}
