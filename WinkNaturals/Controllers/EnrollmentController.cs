using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
        private readonly IShoppingService _shoppingService;

        public EnrollmentController(IEnrollmentService enrollmentService, IShoppingService shoppingService)
        {
            _enrollmentService = enrollmentService;
            _shoppingService = shoppingService;
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
        /// <summary>
        /// GetDistributors
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDistributors")]
        public IActionResult GetDistributors(TransactionalRequestModel transactionRequests)
        {
            return Ok(_enrollmentService.GetDistributors(Identity.CustomerID));
        }
        /// <summary>
        /// Get ProductList data
        /// </summary>
        /// <returns></returns>
        [HttpGet("ProductList")]
        public List<ShopProductsResponse> ProductList(int categoryID, int sortBy)
        {
            categoryID = categoryID == 0 ? 1 : categoryID;
            var categories = new List<ShopProductsResponse>();
            GetItemListRequest itemsRequest;
            var items = new List<ShopProductsResponse>();
            itemsRequest = new GetItemListRequest
            {
                IncludeChildCategories = true,
                CategoryID = categoryID,
                SortBy = sortBy
            };
            items = _shoppingService.GetItems(itemsRequest, false).OrderBy(c => c.SortOrder).ToList();
            return items;
        }


    }
}
