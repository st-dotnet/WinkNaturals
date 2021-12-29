using Exigo.Api.Client;
using System;
using System.ComponentModel.DataAnnotations;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.Interfaces;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Infrastructure.Services.ExigoService.CreditCard
{
    public class CreditCard : ICreditCard
    {
        public CreditCard()
        {
            Type = CreditCardType.New;
            BillingAddress = new Address();
            ExpirationMonth = DateTime.Now.Month;
            ExpirationYear = DateTime.Now.Year;
            AutoOrderIDs = new int[0];
        }
        public CreditCard(CreditCardType type)
        {
            Type = type;
            BillingAddress = new Address();
            ExpirationMonth = DateTime.Now.Month;
            ExpirationYear = DateTime.Now.Year;
        }

        public CreditCardType Type { get; set; }
        [Required, Display(Name = "NameOnCard")]
        public string NameOnCard { get; set; }

        [Required, Display(Name = "CardNumber")]
        public string CardNumber { get; set; }

        [Required, Display(Name = "ExpirationMonth")]
        public int ExpirationMonth { get; set; }

        [Required(ErrorMessageResourceName = "ExpirationYearRequired")]
        public int ExpirationYear { get; set; }

        public string Token { get; set; }
        public string Display { get; set; }



        public string GetToken()
        {

            if (!IsComplete) return string.Empty;

            // Credit Card Tokens should be retrieved via javascript using the exigopayments.js method, not on the server side.
            if (String.IsNullOrEmpty(Token))
            {
                throw new Exception("NO TOKEN PRESENT: Token should be retrieved on front end, review the logic that is getting the Credit Card information since the Token is not currently populated.");
            }
            else
            {
                return Token;
            }

            //return DAL.Payments().FetchCreditCardToken(
            //    this.CardNumber,
            //    this.ExpirationMonth,
            //    this.ExpirationYear);
        }

        [Required, Display(Name = "CVV")]
        public string CVV { get; set; }

        [Required, DataType("Address")]
        public Address BillingAddress { get; set; }

        public int[] AutoOrderIDs { get; set; }

        public bool MakeItPrimary { get; set; }    
        public DateTime ExpirationDate
        {
            get { return new DateTime(this.ExpirationYear, this.ExpirationMonth, DateTime.DaysInMonth(this.ExpirationYear, this.ExpirationMonth)); }
        }

        public bool IsExpired
        {
            get { return this.ExpirationDate < DateTime.Now; }
        }
        public bool IsComplete
        {
            get
            {
                if (string.IsNullOrEmpty(NameOnCard)) return false;
                if (ExpirationMonth == 0) return false;
                if (ExpirationYear == 0) return false;
                if (!BillingAddress.IsComplete) return false;

                return true;
            }
        }
        public bool IsValid
        {
            get
            {
                if (!IsComplete) return false;
                if (IsExpired) return false;
                // This can't work because Card Number does not belong on the server side, the Token is retrieved before the Credit Card is passed to the back end.
                //if (!IsTestCreditCard && !GlobalUtilities.ValidateCreditCard(CardNumber)) return false;

                return true;
            }
        }
        public bool IsUsedInAutoOrders
        {
            get { return this.AutoOrderIDs.Length > 0; }
        }
        public bool IsTestCreditCard
        {
            get { return this.Display == "9696" || this.CardNumber == "9696969696969696"; }
        }

        public AutoOrderPaymentType AutoOrderPaymentType
        {
            get
            {
                switch (this.Type)
                {
                    case CreditCardType.Primary:
                    default: return AutoOrderPaymentType.PrimaryCreditCard;

                    case CreditCardType.Secondary: return AutoOrderPaymentType.SecondaryCreditCard;
                }
            }
        }

    }

}