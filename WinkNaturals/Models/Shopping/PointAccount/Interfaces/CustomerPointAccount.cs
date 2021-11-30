using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;

namespace WinkNaturals.Models.Shopping.PointAccount.Interfaces
{
    public interface ICustomerPointAccount
    {
        object GetCustomerPointAccounts(int customerID, int pointAccountID);

        object SaveNewCustomerCreditCard(int customerID, CreditCard card);




    }
}
