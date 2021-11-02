using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static WinkNatural.Services.Services.PaymentService;

namespace WinkNatural.Services.DTO.Shopping
{
    public class ProcessPaymentMethodTransactionResponse
    {
        /// <summary>
        /// <c>TransactionInformation</c> object for each transaction that was processed.
        /// </summary>
        public System.Transactions.TransactionInformation Transaction
        {
            get; set;
        }

        /// <summary>
        /// <c>Result</c> structure for giving the result of the transaction.
        /// </summary>
        public Result RequestResult
        {
            get; set;
        }
    }
}
