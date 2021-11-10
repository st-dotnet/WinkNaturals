using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Interfaces.PointAccount
{
    public class CustomerPointAccount: PointAccount
    {
        public int CustomerID { get; set; }
        public decimal Balance { get; set; }
    }
}
