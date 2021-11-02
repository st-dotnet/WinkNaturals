using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinkNatural.Web.Services.DTO
{
    public class AuthPaymentModel
    {
        public class MerchantAuthentication
        {
            public string name { get; set; }
            public string transactionKey { get; set; }

        }

        public class PaymentProfile
        {
            public string paymentProfileId { get; set; }

        }

        public class Profile
        {
            public string customerProfileId { get; set; }
            public PaymentProfile paymentProfile { get; set; }

        }

        public class LineItem
        {
            public string itemId { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string quantity { get; set; }
            public string unitPrice { get; set; }

        }

        public class LineItems
        {
            public LineItem lineItem { get; set; }

        }

        public class TransactionRequest
        {
            public string transactionType { get; set; }
            public string amount { get; set; }
            public Profile profile { get; set; }
            public LineItems lineItems { get; set; }

        }

        public class CreateTransactionRequest
        {
            public MerchantAuthentication merchantAuthentication { get; set; }
            public string refId { get; set; }
            public TransactionRequest transactionRequest { get; set; }

        }

        public class PaymentRequest
        {
            public CreateTransactionRequest createTransactionRequest { get; set; }

        }


        public class Message2
        {
            public string code { get; set; }
            public string description { get; set; }

        }

        public class UserField
        {
            public string name { get; set; }
            public string value { get; set; }

        }

        public class TransactionResponse
        {
            public string responseCode { get; set; }
            public string authCode { get; set; }
            public string avsResultCode { get; set; }
            public string cvvResultCode { get; set; }
            public string cavvResultCode { get; set; }
            public string transId { get; set; }
            public string refTransID { get; set; }
            public string transHash { get; set; }
            public string accountNumber { get; set; }
            public string accountType { get; set; }
            public List<Message2> messages { get; set; }
            public List<UserField> userFields { get; set; }

        }

        public class Message
        {
            public string code { get; set; }
            public string text { get; set; }

        }

        public class Messages
        {
            public string resultCode { get; set; }
            public List<Message> message { get; set; }

        }

        public class AddPaymentResponse
        {
            public TransactionResponse transactionResponse { get; set; }
            public string refId { get; set; }
            public Messages messages { get; set; }

        }

    }
}
