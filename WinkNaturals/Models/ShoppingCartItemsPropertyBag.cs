﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models.Shopping.Interfaces;

namespace WinkNaturals.Models
{
    public class ShoppingCartItemsPropertyBag : BasePropertyBag, ICart
    {
        private string version = "1.0.0";
        private int expires = 31;

        #region Constructors
        public ShoppingCartItemsPropertyBag()
        {
           Expires = expires;
            Items = new ShoppingCartItemCollection();
        }
        #endregion

        #region Properties
        public int CustomerID { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
        public string Domain { get; set; }
        public ShoppingCartItemCollection Items { get; set; }
        #endregion

        #region Methods
        //public override T OnBeforeUpdate<T>(T propertyBag)
        //{
        //    propertyBag.Version = version;

        //    return propertyBag;
        //}
        //public override bool IsValid()
        //{
        //    return this.Version == version;
        //}
        #endregion
    }
}