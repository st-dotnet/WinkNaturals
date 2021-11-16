using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.Interfaces;
using System.Net.Http;
using WinkNaturals.Models;
using Newtonsoft.Json;
using static WinkNaturals.Models.GetPaymentModel;
using WinkNatural.Web.Services.Services;
using Microsoft.Extensions.Configuration;
using WinkNaturals.Setting;
using Microsoft.Extensions.Options;

namespace WinkNaturals.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IOptions<ConfigSettings> _config;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerService _customerService;

        public PaymentController(IOptions<ConfigSettings> config,
            IPaymentService paymentService,
            ICustomerService customerService)
        {
            _config = config;
            _paymentService = paymentService;
            _customerService = customerService;
        }



        [HttpGet("ProcessPayment")]
        public IActionResult ProcessPayment(WinkPaymentRequest winkPaymentRequest)
        {
            return Ok(_paymentService.ProcessPayment(winkPaymentRequest));
        }

        [HttpPost("CreateCustomerProfile")]
        public IActionResult CreateCustomerProfile(GetPaymentRequest model)
        {
            return Ok(_paymentService.CreateCustomerProfile(model));
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

    }
}
