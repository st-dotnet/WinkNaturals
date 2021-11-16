namespace WinkNatural.Web.Services.Services
{
    public class PaymentsResponse
    {

    }

    public class AddCardResponse
    {
        public string Message { get; set; }
        public string ResultCode { get; set; }
        public string TransId { get; set; }
        public string AuthCode { get; set; }
    }

    public class AddCardRequest
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
        public string AuthProfileId { get; set; }
        public string AuthCardId { get; set; }
    }

    public class AddPaymentRequest
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
    }
}
