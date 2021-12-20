using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.Interfaces;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;
        private readonly IShoppingService _shoppingService;

        public int LoyaltyPointAccountId { get { return 1; } }
        public AccountController(IAccountService accountService, IShoppingService shoppingService)
        {
            _accountService = accountService;
            _shoppingService = shoppingService;
        }
        [HttpGet("Points")]
        public IActionResult Points()
        {
            return Ok(_accountService.LoyaltyPointsService(Identity.CustomerID, LoyaltyPointAccountId));
        }
        [HttpGet("GetShipMethod")]
        public IActionResult GetShipMethod()
        {
            return Ok(_accountService.GetShipMethodsRequest());
        }

        [HttpGet("GetCustomerPointTransactions")]
        public IActionResult GetCustomerPointTransactions()
        {
            return Ok(_accountService.GetCustomerPointTransactions(Identity.CustomerID, LoyaltyPointAccountId));
        }

        //[HttpGet("CreatePointPayment")]
        //public IActionResult CreatePointPayment()
        //{
        //    return Ok(_accountService.CreatePointPayment(Identity.CustomerID, LoyaltyPointAccountId));
        //}
        [HttpGet("GetCustomerOrders_SQL")]
        public IActionResult GetCustomerOrders_SQL()
        {
            return Ok(_accountService.GetCustomerOrders_SQL(Identity.CustomerID, LoyaltyPointAccountId));
        }
        [HttpGet("AddressList")]
        public IActionResult AddressList()
        {
            return Ok(_shoppingService.GetCustomerAddress(Identity.CustomerID).Where(c => c.IsComplete).ToList());
        }

        [HttpGet("EditAddress")]
        public IActionResult EditAddress(AddressType addresstype)
        {
            return Ok(_shoppingService.GetCustomerAddress(Identity.CustomerID).Where(c => c.AddressType == addresstype).FirstOrDefault());
        }
        /// <summary>
        /// GetCustomerBilling
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCustomerBilling")]///{itemCode:string}
        public IActionResult GetCustomerBilling()
        {
            return Ok(_accountService.GetCustomerBilling(Identity.CustomerID));
        }

        [HttpPost("SaveAddress/{CustomerID:int}")]
        public IActionResult SaveAddress(int CustomerID, ShippingAddress address)
        {
            return Ok(_shoppingService.AddUpdateCustomerAddress(CustomerID, address));
        }

      
    }
}
