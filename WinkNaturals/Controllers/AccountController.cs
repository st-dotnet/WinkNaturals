using Exigo.Api.Client;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
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

        private readonly IWebHostEnvironment _webHostEnvironment;
        public int LoyaltyPointAccountId { get { return 1; } }
        public AccountController(IAccountService accountService, IShoppingService shoppingService, ICustomerAutoOreder customerAutoOreder, ICustomerService customerService
            , IWebHostEnvironment _webHostEnvironment)
        {
            _accountService = accountService;
            _shoppingService = shoppingService;
            _customerAutoOreder = customerAutoOreder;
            _customerService = customerService;
            this._webHostEnvironment = _webHostEnvironment;
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
            //   return Ok(await _accountService.GetCustomerAutoOrdersList(Identity.CustomerID));

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
        [AllowAnonymous]
        [HttpGet("GetOrderInvoice/{orderId}")]
        public async Task<IActionResult> GetOrderInvoice(int orderId)
        {
            var invoiceHtmlResponse = await _accountService.GetOrderInvoice(orderId);
            //  return Ok(Base64Decode(Convert.ToBase64String(invoiceHtmlResponse.InvoiceData)));
            ///  var htmlString = System.Text.Encoding.Default.GetString(invoiceHtmlResponse.InvoiceData);
            return Ok(invoiceHtmlResponse.InvoiceData);

        }

        [HttpGet("SetPrimaryAddress/{type}")]
        public async Task<IActionResult> SetPrimaryAddress(AddressType type)
        { 
            return Ok(await _shoppingService.SetCustomerPrimaryAddress(Identity.CustomerID, type));
        }

        /// <summary>
        /// Make CreditCard Primary
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        [HttpPost("MakeCreditCardPrimary")]
        public async Task<IActionResult> MakeCreditCardPrimary(CreditCard card)
        {
            var response = await _accountService.MakeCreditCardAsPrimary(Identity.CustomerID, card, card.Type);
            if (response)
                return Ok(_accountService.GetCustomerBilling(Identity.CustomerID));
            else
                return Ok(false);
        }

        /// <summary>
        /// Save customer address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="makePrimary"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        [HttpPost("SaveAddress")]
        public async Task<IActionResult> SaveAddress(Address address, bool makePrimary)
        { 
            //if need to make the address as primary address
            if (makePrimary)
            {
                address.PrimaryCard = true;
                await _shoppingService.SetCustomerPrimaryAddress(Identity.CustomerID, address.AddressType);
            } 
            else // save the address
            {
                address.PrimaryCard = false;
                address = await _shoppingService.SetCustomerAddressOnFile(Identity.CustomerID, address);
            } 
            return Ok(address);
        }

        /// <summary>
        /// Update customer address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="makePrimary"></param>
        /// <param name="isEdit"></param>
        /// <returns></returns>
        [HttpPost("UpdateAddress")]
        public async Task<IActionResult> UpdateAddress(Address address, bool isEdit)
        {
           return Ok(await _shoppingService.SetCustomerAddressOnFile(Identity.CustomerID, address, isEdit));
        }

        /// <summary>
        /// EditCreditCard
        /// </summary>
        /// <returns></returns>
        [HttpPost("EditCreditCard")]
        public IActionResult EditCreditCard(string type)
        {
            return Ok(_accountService.GetCustomerBilling(Identity.CustomerID).Result.Where(c => c is CreditCard && ((CreditCard)c).Type.ToString() == type).FirstOrDefault());
        }
                
        /// <summary>
        /// MakeAddressPrimary
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpPost("MakeAddressPrimary")]
        public async Task<IActionResult> MakeAddressPrimary(Address address)
        {
            //Make address as Primary address
            return Ok(await _accountService.MakeAddressAsPrimary(Identity.CustomerID, address));
            //Get all 
        }

        /// <summary>
        /// Edit Subcription
        /// </summary>
        /// <param name="autoOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("EditSubcription/{autoOrderId:int}")]
        public async Task<IActionResult> EditSubcription(int autoOrderId)
        {
            return Ok(await _accountService.EditSubcription(Identity.CustomerID, autoOrderId));
        }
    }
}
