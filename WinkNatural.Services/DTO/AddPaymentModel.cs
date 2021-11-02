using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Services.DTO
{
    public class AddPaymentModel
    {
        public decimal PaymentAmount { get; set; }
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public int PaymentMethod { get; set; }
        public int CreditCardId { get; set; }
        public string Description { get; set; }
        public int PaymentType { get; set; }
        public string Result { get; set; }
        public bool Approved { get; set; }

        public string quantity { get; set; }
    }
}
