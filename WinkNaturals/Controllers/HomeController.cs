using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Customer;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Models; 
using WinkNaturals.Setting;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;
        private readonly IMapper _mapper;
        private readonly IOptions<ConfigSettings> _config;
        public HomeController(IHomeService homeService, IMapper mapper, IOptions<ConfigSettings> config)
        {
            _homeService = homeService;
            _mapper = mapper;
            _config = config;
        }
        /// <summary>
        /// About
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("About")]
        public async Task<IActionResult> About(ContactModel model)
        {
            try
            {
                var data = _mapper.Map<ContactRequest>(model);
                var response = await _homeService.SendEmail(data);
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Get hpme page reviews from Yotpo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHomePageReviews")]
        public List<HomePageReviewsModel> GetHomePageReviews()
        {
            try
            {
                return _homeService.GetReviews(); 
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            
        }
    }
}
