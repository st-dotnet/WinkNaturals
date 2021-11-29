using Braintree;
using Microsoft.Extensions.Options;
using WinkNaturals.Setting;
using TransactionStatus = Braintree.TransactionStatus;

namespace WinkNaturals.Models.BraintreeService
{
    public class BraintreeService
    {
        private IBraintreeGateway BraintreeGateway { get; set; }
        private readonly IOptions<ConfigSettings> _config;
        //public BraintreeService()
        //{


        //}
        public BraintreeService(IOptions<ConfigSettings> config)
        {
            _config = config;

        }

        public static readonly TransactionStatus[] TransactionSuccessStatuses =
            {
                TransactionStatus.AUTHORIZED,
                TransactionStatus.AUTHORIZING,
                TransactionStatus.SETTLED,
                TransactionStatus.SETTLING,
                TransactionStatus.SETTLEMENT_CONFIRMED,
                TransactionStatus.SETTLEMENT_PENDING,
                TransactionStatus.SUBMITTED_FOR_SETTLEMENT
            };
        public IBraintreeGateway CreateGateway()
        {
            return new BraintreeGateway(
                 _config.Value.BraintreeConfiguration.Environment,
                 _config.Value.BraintreeConfiguration.MerchantId,
                 _config.Value.BraintreeConfiguration.PublicKey,
                 _config.Value.BraintreeConfiguration.PrivateKey

            );
        }
        public IBraintreeGateway GetGateway()
        {
            if (BraintreeGateway == null)
            {
                BraintreeGateway = CreateGateway();
            }

            return BraintreeGateway;
        }

        /// <summary>
        /// Generates the Client Token to use in the client side
        /// </summary>
        /// <returns></returns>
        public string GetClientToken()
        {
            var gateway = GetGateway();
            var clientToken = gateway.ClientToken.Generate();

            return clientToken;
        }

        /// <summary>
        /// Get the Server Token to use it for the Exigo request payment
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="nonce"></param>
        /// <returns></returns>
        public string GetServerToken(int customerID, string nonce)
        {
            var gateway = GetGateway();
            var paypalCustomerRequest = new CustomerRequest
            {
                Id = customerID.ToString()
            };
            Result<Customer> customerResult = gateway.Customer.Create(paypalCustomerRequest);

            var tokenRequest = new PaymentMethodRequest
            {
                PaymentMethodNonce = nonce,
                CustomerId = customerID.ToString()
            };
            Result<PaymentMethod> tokenResult = gateway.PaymentMethod.Create(tokenRequest);

            return tokenResult.Target.Token;
        }
    }
}
