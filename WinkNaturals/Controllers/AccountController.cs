using Exigo.Api.Client;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces;
using static WinkNaturals.Helpers.Constant;
using CreditCard = WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.CreditCard;
namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;
        private readonly IShoppingService _shoppingService;
        private readonly ICustomerAutoOreder _customerAutoOreder;
        private readonly ICustomerService _customerService;

        public int LoyaltyPointAccountId { get { return 1; } }
        public AccountController(IAccountService accountService, IShoppingService shoppingService, ICustomerAutoOreder customerAutoOreder, ICustomerService customerService)
        {
            _accountService = accountService;
            _shoppingService = shoppingService;
            _customerAutoOreder = customerAutoOreder;
            _customerService = customerService;
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
        public async Task<IActionResult> GetCustomerOrders_SQL()
        {
            return Ok(await _accountService.GetCustomerOrders_SQL(Identity.CustomerID, LoyaltyPointAccountId));
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
        public async Task<IActionResult> GetCustomerBilling()
        {
            return Ok(await _accountService.GetCustomerBilling(Identity.CustomerID));
        }
        ///// <summary>
        ///// SaveAddress
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost("SaveAddress")]
        //public async Task<IActionResult> SaveAddress(Address address)
        //{
        //    return Ok(await _shoppingService.AddUpdateCustomerAddress(Identity.CustomerID, address));
        //}

        /// <summary>
        /// GetCustomerAutoOrders
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCustomerAutoOrders")]
        public async Task<IActionResult> GetCustomerAutoOrders()
        {
            return Ok(await _accountService.GetCustomerAutoOrders(Identity.CustomerID));
        }

        /// <summary>
        /// CancelledCustomerOrders_SQL
        /// </summary>
        /// <returns></returns>
        [HttpGet("CancelledCustomerOrders_SQL")]
        public async Task<IActionResult> CancelledCustomerOrders_SQL()
        {
            return Ok(await _accountService.CancelledCustomerOrders_SQL(Identity.CustomerID, LoyaltyPointAccountId));
        }
        /// <summary>
        /// SeachOrderList
        /// </summary>
        /// <returns></returns>
        [HttpGet("SeachOrderList")]
        public async Task<IActionResult> SeachOrderList(int orderid)
        {
            return Ok(await _accountService.SeachOrderList(Identity.CustomerID, orderid));
        }
        /// <summary>
        /// DeclinedCustomerOrders_SQL
        /// </summary>
        /// <returns></returns>
        [HttpGet("DeclinedCustomerOrders_SQL")]
        public async Task<IActionResult> DeclinedCustomerOrders_SQL()
        {
            return Ok(await _accountService.DeclinedCustomerOrders_SQL(Identity.CustomerID, LoyaltyPointAccountId));
        }
        /// <summary>
        /// ShippedCustomerOrders_SQL
        /// </summary>
        /// <returns></returns>
        [HttpGet("ShippedCustomerOrders_SQL")]
        public async Task<IActionResult> ShippedCustomerOrders_SQL()
        {
            return Ok(await _accountService.ShippedCustomerOrders_SQL(Identity.CustomerID, LoyaltyPointAccountId));
        }
        /// <summary>
        /// GetOrderInvoice
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetOrderInvoice")]
        public async Task<IActionResult> GetOrderInvoice(GetOrderInvoiceRequest request)
        {
            return Ok(await _accountService.GetOrderInvoice(request));
        }
       
        [HttpPost("SetPrimaryAddress")]
        public IActionResult SetPrimaryAddress(AddressType type)
        {
            return Ok(_shoppingService.SetCustomerPrimaryAddress(Identity.CustomerID, type));
        }
        [HttpPost("SaveAddress")]
        public async Task<IActionResult> SaveAddress(Address address, bool? makePrimary)
        {
            address = await _shoppingService.SetCustomerAddressOnFile(Identity.CustomerID, address);
            if (makePrimary != null && makePrimary == true)
            {
                await _shoppingService.SetCustomerPrimaryAddress(Identity.CustomerID, address.AddressType);
            }
            return Ok(address);
        }
        /// <summary>
        /// EditCreditCard
        /// </summary>
        /// <returns></returns>
        [HttpPost("EditCreditCard")]
        public IActionResult EditCreditCard(CreditCardType type)
        {
            return Ok(_accountService.GetCustomerBilling(Identity.CustomerID).Result.Where(c => c is CreditCard && ((CreditCard)c).Type == type).FirstOrDefault());
        }
    }
}
