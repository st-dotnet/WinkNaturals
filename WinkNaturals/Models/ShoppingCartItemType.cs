﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models
{
    public enum ShoppingCartItemType
    {
        Order = 0,
        AutoOrder = 1,
        WishList = 2,
        Coupon = 3,
        EnrollmentPack = 4,
        EnrollmentAutoOrderPack = 5
    }
}
