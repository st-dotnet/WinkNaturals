namespace WinkNaturals.Models
{
    public class GetPaymentModel
    {
        public class AddCardModel
        {
            public int CardType { get; set; }
            public string CardNumber { get; set; }
            public int ExpMonth { get; set; }
            public int ExpYear { get; set; }
            public int CustomerId { get; set; }
            public bool Primary { get; set; }
            public bool Active { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ZipCode { get; set; }

            public string Address1 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Country { get; set; }

            public decimal Price { get; set; }

        }
    }
}
