using Exigo.Api.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Models;
using WinkNaturals.Setting.Interfaces;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : BaseController
    {
        private readonly IExigoApiContext _exigoApiContext;
        private readonly ICustomerService _customerService;
        public CustomersController(IExigoApiContext exigoApiContext,ICustomerService customerService)
        {
            _exigoApiContext = exigoApiContext;
            _customerService = customerService;
        }
        [HttpPost("GetCustomer")]
        public async Task<IActionResult> GetCustomer()
        {
            try
            {
                var apiItems = new List<EnrollmentModel>();
                GetItemsRequest req = new GetItemsRequest
                {
                    //We will be requesting three items
                    ItemCodes = new string[3]
                };
                req.ItemCodes[0] = "SK-Q1KIT3-21";
                req.ItemCodes[1] = "SK-Q1KIT2-21";
                req.ItemCodes[2] = "SK-Q1KIT1-21";
                req.CurrencyCode = "usd";
                req.WarehouseID = 1;
                req.PriceType = 1;

                GetItemsResponse res = await _exigoApiContext.GetContext(false).GetItemsAsync(req);
                var result = apiItems;
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        [HttpDelete("DeleteAutoOrder")]
        public IActionResult DeleteAutoOrder(int id)
        {
            return Ok(_customerService.DeleteCustomerAutoOrder(Identity.CustomerID, id));
        }
        [HttpPost("DeleteCreditCard")]
        public async Task<IActionResult> DeleteCreditCard(CreditCardType type)
        {
             await _customerService.DeleteCustomerCreditCard(Identity.CustomerID, type);
            return Ok();
        }
    }
}
