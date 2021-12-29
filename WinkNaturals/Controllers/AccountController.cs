﻿using Exigo.Api.Client;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
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

        [HttpPost("SetPrimaryAddress")]
        public IActionResult SetPrimaryAddress(string type)
        {
            AddressType addresstype;
            Enum.TryParse<AddressType>(type, out addresstype);
            return Ok(_shoppingService.SetCustomerPrimaryAddress(Identity.CustomerID, addresstype));
        }

        [HttpPost("SaveAddress")]
        public async Task<IActionResult> SaveAddress(Address address, bool makePrimary)
        { 
            //if need to make the address as primary address
            if (makePrimary)
                await _shoppingService.SetCustomerPrimaryAddress(Identity.CustomerID, address.AddressType);
            else // save the address
                address = await _shoppingService.SetCustomerAddressOnFile(Identity.CustomerID, address);
            return Ok(address);
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

        [HttpGet]
        private ActionResult HtmlToPdf(string data)
        {
            var newFileGuid = Guid.NewGuid();
            var url = $"{newFileGuid}.html";
            //System.IO.File.WriteAllText(url, data);
            string webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, url);
            System.IO.File.WriteAllText(path, data);
            var fileName = $"{Guid.NewGuid()}.pdf";
            var outputPath = Path.Combine(webRootPath, fileName);
            try
            {
                using (FileStream htmlSource = System.IO.File.Open(path, FileMode.Open))
                using (FileStream pdfDest = System.IO.File.Open(outputPath, FileMode.OpenOrCreate))
                {
                    ConverterProperties converterProperties = new ConverterProperties();
                    converterProperties.SetFontProvider(new DefaultFontProvider(true, true, true));
                    HtmlConverter.ConvertToPdf(htmlSource, pdfDest, converterProperties);
                }
            }
            catch (Exception ex)
            {
               // _logger.LogDebug(1, ex.ToString());
            }

            var stream = System.IO.File.OpenRead(outputPath);
            //System.IO.File.Delete(path);

            return new FileStreamResult(stream, "application/pdf");
        }

        ///// <summary>
        ///// SaveCreditCard
        ///// </summary>
        ///// <returns></returns>
        ///// 
        //[HttpPost("SaveCreditCard")]
        //public async Task<IActionResult> SaveCreditCard(CreditCard card)
        //{
        //    try
        //    {
        //        card = await _accountService.SetCustomerCreditCard(Identity.CustomerID, card);
        //        if (card.Type == CreditCardType.Primary)
        //        {
        //            var updateCustomerRequest = new UpdateCustomerRequest
        //            {
        //                CustomerID = Identity.CustomerID,
        //                Field1 = "1"
        //            };
        //            var transactionResponse = await _customerService.UpdateCustomer(updateCustomerRequest);
        //        }
        //        else
        //        {
        //            var updateCustomerRequest = new UpdateCustomerRequest
        //            {
        //                CustomerID = Identity.CustomerID,
        //                Field2 = "1"
        //            };
        //            var transactionResponse = await _customerService.UpdateCustomer(updateCustomerRequest);
        //        }
        //        return Ok(card);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
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
    }
}
