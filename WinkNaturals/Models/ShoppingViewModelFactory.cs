using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Checkout;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Models
{
    public static class ShoppingViewModelFactory
    {
        public static T Create<T>(ShoppingCartCheckoutPropertyBag propertyBag) where T : IShoppingViewModel
        {
            var viewModel = Activator.CreateInstance<T>();

            viewModel.PropertyBag = propertyBag;

            return viewModel;
        }
    }
}
