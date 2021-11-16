namespace WinkNatural.Web.Services.Services
{
    public class BaseCommerceClient
    {
        public BaseCommerceClient(string user, string password, string key)
        { }

        public BankCardTransaction processBankCardTransaction(BankCardTransaction bankCardTransaction)
        {
            //BankCardTransaction bcx = new BankCardTransaction();
            //bcx.status = "success";
            //bcx.paymentId = 1;
            bankCardTransaction.status = "success";
            bankCardTransaction.paymentId = 1;
            return bankCardTransaction;
        }
    }
}
