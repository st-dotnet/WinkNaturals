using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
using WinkNatural.Web.Services.Utilities;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : BaseController
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        /// <summary>
        /// Get packs data
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetPacks")]
        public IActionResult GetPacks()
        {
            try
            {
                var items = _enrollmentService.GetItems();

                foreach (var item in items)
                {
                    item.ProductImage = ProductImageUtility.GetProductImageUtility(item.LargeImageUrl);
                }
                items = items.OrderByDescending(x => x.Price).ToList();
                return Ok(items);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// SubmitCheckout
        /// </summary>
        /// <returns></returns>
        [HttpPost("SubmitCheckout")]
        public IActionResult SubmitCheckout(TransactionalRequestModel transactionRequests)
        {
            return Ok(_enrollmentService.SubmitCheckout(transactionRequests, Identity.CustomerID));
        }
    }
}
