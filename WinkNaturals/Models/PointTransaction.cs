using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models
{
    public class PointTransaction
    {
        public int PointTransactionID { get; set; }
        public int CustomerID { get; set; }

        public int PointAccountID { get; set; }
        public int PointTransactionTypeID { get; set; }

        public decimal Amount { get; set; }
        public string FormattedAmount => Amount.ToString("C2");

        public DateTime TransactionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? OrderID { get; set; }
        public string Reference { get; set; }
        public decimal Balance { get; set; }
        public string FormattedBalance => Balance.ToString("C2");

        public PointTransactionType PointTransactionType { get; set; }
        public PointAccount PointAccount { get; set; }
    }
}
