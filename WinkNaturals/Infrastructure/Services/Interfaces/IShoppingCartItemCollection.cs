using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Common.Utils.Enum;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Setting.Interfaces
{
    public interface IShoppingCartItemCollection
    {
        void Add(IShoppingCartItem item);

        void Update(Guid id, decimal quantity);
        void Update(IShoppingCartItem item);

        void Remove(Guid id);
        void Remove(ShoppingCartItemType type);
    }
}
