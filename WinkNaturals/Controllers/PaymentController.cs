using Exigo.Api.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services;
using WinkNaturals.Models.BraintreeService;
using WinkNaturals.Setting;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IOptions<ConfigSettings> _config;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerService _customerService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IShoppingService _shoppingService;

        public PaymentController(IOptions<ConfigSettings> config,
            IPaymentService paymentService,
            ICustomerService customerService,IEnrollmentService enrollmentService, IShoppingService shoppingService)
        {
            _config = config;
            _paymentService = paymentService;
            _customerService = customerService;
            _enrollmentService = enrollmentService;
            _shoppingService = shoppingService;
        }

        //   [HttpGet("GenerateCreditCardToken/{cardNumber}")]
        [HttpGet("GenerateCreditCardToken")]
        public IActionResult GenerateCreditCardToken(string cardNumber)
        {
            return Ok(_paymentService.GenerateCreditCardToken(cardNumber));
        }
        [HttpGet("ProcessPayment")]
        public IActionResult ProcessPayment(WinkPaymentRequest winkPaymentRequest)
        {
            return Ok(_paymentService.ProcessPayment(winkPaymentRequest));
        }

        [HttpPost("CreateCustomerProfile")]
        public async Task<IActionResult> CreateCustomerProfile(GetPaymentRequest model)
        {
            return Ok(await _paymentService.CreateCustomerProfile(model));
        }

        // This code is for make payment using propay account
        [HttpPost("CreatePaymentUsingProPay")]
        public IActionResult CreatePaymentUsingProPay(GetPaymentRequest getPaymentProPayModel)
        {
            return Ok(_paymentService.ProcessPaymentMethod(getPaymentProPayModel));
        }

        [HttpPost("CreatePaymentUsingAuthorizeNet")]
        public IActionResult CreatePaymentUsingAuthorizeNet(AddPaymentModel model)
        {
            return Ok(_paymentService.PaymentUsingAuthorizeNet(model));
        }

        [HttpGet("GetClientToken")]
        public IActionResult GetClientToken()
        {
            var braintreeService = new BraintreeService(_config);
            var paypalClientToken = braintreeService.GetClientToken();
            return Ok(new
            {
                token = paypalClientToken
            });
        }
        /// <summary>
        /// SaveCreditCard
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost("SaveCreditCard")]
        public async Task<IActionResult> SaveCreditCard(SetAccountCreditCardTokenRequest card)
        {
            //var response = _enrollmentService.SetCustomerCreditCard(Identity.CustomerID, card);
            //if (card.CreditCardType == CreditCardType.Primary.ToString())
            //{
            //    var updateCustomerRequest = new UpdateCustomerRequest
            //    {
            //        CustomerID = Identity.CustomerID,
            //        Field1 = "1"
            //    };
            //    return Ok(_shoppingService.UpdateCustomer(updateCustomerRequest));
            //}
            //else
            //{
            //    var updateCustomerRequest = new UpdateCustomerRequest
            //    {
            //        CustomerID = Identity.CustomerID,
            //        Field2 = "1"
            //    };
            //    return Ok( _shoppingService.UpdateCustomer(updateCustomerRequest));
            //}
            return Ok(await _enrollmentService.SetCustomerCreditCard(card,Identity.CustomerID));

        }


    }
}
