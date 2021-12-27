using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
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
        [HttpGet("GetOrderInvoice/{orderId}")]
        public async Task<IActionResult> GetOrderInvoice(int orderId)
        {
            var invoiceHtmlResponse = await  _accountService.GetOrderInvoice(orderId);
            var htmlString = System.Text.Encoding.Default.GetString(invoiceHtmlResponse.InvoiceData); 
            return HtmlToPdf(htmlString);

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


        [HttpGet]
        public ActionResult HtmlToPdf(string data)
        {
            var url = @"assets\slip.html";
            System.IO.File.WriteAllText(url, data);
            string webRootPath = _webHostEnvironment.WebRootPath;
            var path = Path.Combine(webRootPath, url);
            var fileName = $"Pdf\\{Guid.NewGuid()}.pdf";
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
            return new FileStreamResult(stream, "application/pdf");
        }
    }
}
