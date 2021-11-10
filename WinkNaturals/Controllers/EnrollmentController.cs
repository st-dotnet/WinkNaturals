using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Services.Interfaces;
using WinkNatural.Web.Services.Utilities;
using WinkNaturals.Models;

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
    }
}
