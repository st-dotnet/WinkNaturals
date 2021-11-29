namespace WinkNatural.Web.Services.DTO.Shopping
{
    public class TransactionDetail
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string streetAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }
        public string newStreetAddress { get; set; }
        public string newCity { get; set; }
        public string newState { get; set; }
        public string newZip { get; set; }
        public string newCountry { get; set; }
        public string cardName { get; set; }
        public string cardNumber { get; set; }
        public string expiryMonth { get; set; }
        public string expiryYear { get; set; }
        public string cardCVV { get; set; }
        public string isMakePrimaryCard { get; set; }
    }
}
