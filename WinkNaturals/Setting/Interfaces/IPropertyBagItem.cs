using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Setting.Interfaces
{
    public interface IPropertyBagItem
    {
        List<Item> GetItems(IEnumerable<ShoppingCartItem> shoppingCartItems, IOrderConfiguration configuration, int languageID, int _priceTypeID = 0);
    }
}
