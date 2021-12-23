using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Shopping.CalculateOrder;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Infrastructure.Services.DTO
{
    public class ManageAutoOrderViewModel
    {
        public ManageAutoOrderViewModel()
        {
            AutoOrder = new AutoOrder();
            NewCreditCard = new CreditCard();
            AvailableShipMethods = new List<ShipMethod>();
        }

        public List<ShipMethod> AvailableShipMethods { get; set; }

        public AutoOrder AutoOrder { get; set; }
        public CreditCard NewCreditCard { get; set; }
        public IEnumerable<IItem> AvailableProducts { get; set; }
        public IEnumerable<IPaymentMethod> AvailablePaymentMethods { get; set; }
        public IEnumerable<DateTime> AvailableStartDates { get; set; }
        public bool UsePointAccount { get; set; }
        public bool IsExistingAutoOrder { get { return AutoOrder != null && AutoOrder.AutoOrderID != 0; } }
    }
}
