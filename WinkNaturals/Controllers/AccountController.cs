using AutoMapper;
using Exigo.Api.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Models.BraintreeService;
using WinkNaturals.Models.Shopping.Checkout;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces.PointAccount;
using WinkNaturals.Models.Shopping.PointAccount.Interfaces;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;
using WinkNaturals.Utilities.Common;
using WinkNaturals.WebDrip;
using System.Threading.Tasks;
using WinkNaturals.Helpers;
using WinkNaturals.Utilities.WebDrip;
using static Dapper.SqlMapper;
using WinkNaturals.Infrastructure.Services.Interfaces;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        public int LoyaltyPointAccountId { get { return 1; } }
        /// <summary>
        /// Points
        /// </summary>
        /// <returns></returns>
        [HttpGet("Points")]
        public IActionResult Points()
        {
            return Ok(_accountService.LoyaltyPointsService(Identity.CustomerID, LoyaltyPointAccountId));
          
        }
    }
}
