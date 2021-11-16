using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Checkout;

namespace WinkNaturals.Models.Shopping.Interfaces
{
  public interface IShoppingViewModel
    {
        ShoppingCartCheckoutPropertyBag PropertyBag { get; set; }
        string[] Errors { get; set; }
    }
}
