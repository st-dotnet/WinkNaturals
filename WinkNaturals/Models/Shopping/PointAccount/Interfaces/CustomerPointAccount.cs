using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.ExigoService.AutoOrder;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Models.Shopping.Interfaces.PointAccount;

namespace WinkNaturals.Models.Shopping.PointAccount.Interfaces
{
   public interface ICustomerPointAccount
    {
      object  GetCustomerPointAccounts(int customerID, int pointAccountID);

        object SaveNewCustomerCreditCard(int customerID, CreditCard card);

       


    }
}
