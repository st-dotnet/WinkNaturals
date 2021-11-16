using System;
using WinkNatural.Web.Services.DTO;
namespace WinkNatural.Web.Services.Services
{
    public class BankCardTransaction
    {
        public void setType(BankCardTransactionEnum type)
        {
            Console.WriteLine(type);
        }
        public void setCardName(string nameOnCard)
        {
            Console.WriteLine(nameOnCard);
        }
        public void setCardNumber(string cardNumber)
        {
            Console.WriteLine(cardNumber);
        }
        public void setCardExpirationMonth(string month)
        {
            Console.WriteLine(month);
        }
        public void setCardExpirationYear(string year)
        {
            Console.WriteLine(year);
        }
        public void setAmount(double amount)
        {
            Console.WriteLine(amount);

        }
        public string status { get; set; }
        public int paymentId { get; set; }
    }


}
