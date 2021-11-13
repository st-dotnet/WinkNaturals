using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Models.Shopping.Orders
{
    public class AutoOrderPaymentTypes
    {
        /// <summary>
        ///	Auto Order Payment Type 1
        /// </summary>
        public const int PrimaryCreditCardOnFile = 1;
        /// <summary>
        ///	Auto Order Payment Type 2
        /// </summary>
        public const int SecondaryCreditCardOnFile = 2;
        /// <summary>
        ///	Auto Order Payment Type 3
        /// </summary>
        public const int DebitCheckingAccount = 3;
        /// <summary>
        ///	Auto Order Payment Type 4
        /// </summary>
        public const int CustomerWillSendPayment = 4;
        /// <summary>
        ///	Auto Order Payment Type 6
        /// </summary>
        public const int PrimaryWalletAccount = 6;
        /// <summary>
        ///	Auto Order Payment Type 7
        /// </summary>
        public const int SecondaryWalletAccount = 7;
    }
}