using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Utilities.Common.Exception;

namespace WinkNaturals.Models
{
    public class ShoppingCartItemCollection : List<ShoppingCartItem>,  IShoppingCartItemCollection
    {
        public void Add(IShoppingCartItem item)
        {

            var newItem = new ShoppingCartItem(item);

            // Don't process items with no quantities
            if (newItem.Quantity == 0) return;

            // Get a list of all items that have the same item code and type.
            var preExistingItems = this.FindAll(i =>
                  i.ItemCode == newItem.ItemCode
            && i.Type.Equals(newItem.Type)
              && i.DynamicKitCategory == newItem.DynamicKitCategory
             && i.ParentItemCode == newItem.ParentItemCode);

            // If we returned any existing items that match the item code and type, we need to add to those existing items.
            if (preExistingItems.Count() > 0)
            {
                // Loop through each item found.
                preExistingItems.ForEach(i =>
                {
                    // Add the new quantity to the existing item code.
                    // Note that the only thing we are adding to the existing item code is the new quantity.
                    i.Quantity = i.Quantity + newItem.Quantity;
                });
            }

            // If we didn't find any existing items matching the item code or type, let's add it to the ShoppingBasketItemsCollection
            else
            {
                base.Add(newItem);
            }
        }
        public void Update(Guid id, decimal quantity)
        {
            var item = this.Where(c => c.ID == id).FirstOrDefault();
            if (item == null) return;

            // Remove the item if it is an invalid quantity
            if (quantity > 0)
            {
                item.Quantity = quantity;
            }
            else
            {
                this.Remove(item.ID);
            }
        }
        public void Update(IShoppingCartItem item)
        {
            var cartitem = new ShoppingCartItem(item);
            var oldItem = this.Where(c => c.ID == cartitem.ID).FirstOrDefault();
            if (oldItem == null) return;

            // Remove the old item
            this.Remove(oldItem.ID);

            // If we have a valid quantity, add the new item
            if (item.Quantity > 0)
            {
                this.Add(item);
            }
        }

        public void Remove(Guid id)
        {
            var matchingItems = this.Where(item => item.ID == id).ToList();
            foreach (var item in matchingItems)
            {
                base.Remove(item);
            }

            DeleteOrphanDynamicKitMembers();
        }
        public void Remove(ShoppingCartItemType type)
        {
            var matchingItems = this.Where(item => item.Type.Equals(type)).ToList();
            foreach (var item in matchingItems)
            {
                base.Remove(item);
            }

            DeleteOrphanDynamicKitMembers();
        }

        public void DeleteOrphanDynamicKitMembers()
        {
            List<ShoppingCartItem> itemsToDelete = new List<ShoppingCartItem>();
            foreach (var item in this)
            {
                if (item.IsDynamicKitMember)
                {
                    var existingParents = this.FindAll(i => i.ItemCode == item.ParentItemCode && !i.IsDynamicKitMember && i.Type == item.Type);
                    if (existingParents.Count == 0)
                    {
                        itemsToDelete.Add(item);
                    }
                }
            }

            foreach (var item in itemsToDelete)
            {
                base.Remove(item);
            }
        }

        public bool CheckItemBackOffice(IShoppingCartItem item)
        {
            var newItem = new ShoppingCartItem(item);

            // Don't process items with no quantities
            if (newItem.Quantity == 0) return false;

             var fieldMax = Convert.ToInt32(item.Field4);
       
             // Get a list of all items that have the same item code and type.
             var preExistingItems = this.FindAll(i =>
                  i.ItemCode == newItem.ItemCode
               && i.Type.Equals(newItem.Type)
               && i.DynamicKitCategory == newItem.DynamicKitCategory
               && i.ParentItemCode == newItem.ParentItemCode);
            if (preExistingItems.FirstOrDefault() != null && (preExistingItems.FirstOrDefault().Quantity + newItem.Quantity) > fieldMax && fieldMax > 0)
                return false;
            else
                return true;
        }
    }
}