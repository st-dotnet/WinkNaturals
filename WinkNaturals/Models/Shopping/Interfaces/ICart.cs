using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface ICart
    {
        ShoppingCartItemCollection Items { get; set; }
    }
}
