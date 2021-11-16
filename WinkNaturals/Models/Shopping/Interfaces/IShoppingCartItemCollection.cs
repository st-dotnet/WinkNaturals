using System;

namespace WinkNaturals.Models.Shopping.Interfaces
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
