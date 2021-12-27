
using System;
using System.ComponentModel.DataAnnotations;
using WinkNaturals.Infrastructure.Services.ExigoService.BankAccount.Interfaces;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.Interfaces;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Infrastructure.Services.ExigoService.BankAccount
{
    public class BankAccount : IBankAccount
    {
        public BankAccount()
        {
            Type = BankAccountType.New;
            BillingAddress = new Address();
            AutoOrderIDs = new int[0];
        }
        public BankAccount(BankAccountType type)
        {
            Type = type;
            BillingAddress = new Address();
        }

        [Required]
        public BankAccountType Type { get; set; }

        [Required(ErrorMessageResourceName = "BankAccountNameRequired"), Display(Name = "NameOnAccount")]
        public string NameOnAccount { get; set; }

        [Required(ErrorMessageResourceName = "BankNameRequired"), Display(Name = "BankName")]
        public string BankName { get; set; }

        [Required(ErrorMessageResourceName = "AccountNumberRequired"), Display(Name = "AccountNumber")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessageResourceName = "RoutingNumberRequired"), Display(Name = "RoutingNumber")]
        public string RoutingNumber { get; set; }

        [Required, DataType("Address")]
        public Address BillingAddress { get; set; }

        public int[] AutoOrderIDs { get; set; }

        public bool IsComplete
        {
            get
            {
                if (string.IsNullOrEmpty(NameOnAccount)) return false;
                if (string.IsNullOrEmpty(BankName)) return false;
                if (string.IsNullOrEmpty(AccountNumber)) return false;
                if (string.IsNullOrEmpty(RoutingNumber)) return false;
                if (!BillingAddress.IsComplete) return false;

                return true;
            }
        }
        public bool IsValid
        {
            get
            {
                if (!IsComplete) return false;

                return true;
            }
        }
        public bool IsUsedInAutoOrders
        {
            get { return this.AutoOrderIDs.Length > 0; }
        }

        public Exigo.Api.Client.AutoOrderPaymentType AutoOrderPaymentType
        {
            get
            {
                switch (this.Type)
                {
                    case Helpers.Constant.BankAccountType.Primary:
                    default: return Exigo.Api.Client.AutoOrderPaymentType.CheckingAccount;
                }
            }
        }


    }
}