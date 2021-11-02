using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Web.Services.DTO
{
    public class GetPaymentRequest
    {
        public int CardType { get; set; }
        public string CardNumber { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public int CustomerId { get; set; }
        public bool Primary { get; set; }
        public bool Active { get; set; }
        public string FirstName { get; set; }
        public string ZipCode { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public decimal Price { get; set; }
        public int CVV { get; set; }
        public string EmailAddress { get; set; }
        public string ExternalId1 { get; set; }
        public string ExternalId2 { get; set; }
    }
}
