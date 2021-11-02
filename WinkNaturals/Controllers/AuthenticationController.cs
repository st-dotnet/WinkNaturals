using AutoMapper;
using Exigo.Api.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using WinkNatural.Web.Common;
using WinkNatural.Web.Common.Utils.Enum;
using WinkNatural.Web.Services.DTO.Customer;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Models;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;
using static WinkNatural.Web.Services.Services.AuthenticateService;

namespace WinkNatural.Web.WinkNaturals.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticateService _authenticateService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<ConfigSettings> _configSettings;
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IGetCurrentMarket _getCurrentMarket;
        private readonly ICustomerService _customerService;

        public AuthenticationController(IAuthenticateService authenticate,
            IMapper mapper, IHttpContextAccessor httpContextAccessor, 
            IOptions<ConfigSettings> configSettings,
            IExigoApiContext exigoApiContext, IGetCurrentMarket getCurrentMarket, ICustomerService customerService)
        {
            _authenticateService = authenticate;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _configSettings = configSettings;
            _exigoApiContext = exigoApiContext;
            _getCurrentMarket = getCurrentMarket;
            _customerService = customerService;
        }
      
        #region Customer

        /// <summary>
        /// Create customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer(CustomerCreateModel model)
        {
            try
            {
                //var captchaResponse = ReCaptchaValidator.IsValid(model.CaptchaResponse);
                //if (string.IsNullOrEmpty(model.CaptchaResponse))
                //{
                //    return BadRequest(new
                //    {
                //        ErrorMessage = "Please verify you are human"
                //    });
                //}
                //else if (!captchaResponse.Success)
                //{
                //    var errors = string.Empty;
                //    foreach (string err in captchaResponse.ErrorCodes)
                //    {
                //        errors += $"{err},";
                //    }
                //    return BadRequest(new
                //    {
                //        ErrorMessage = errors
                //    });
                //}
                //start

                var cookieValueFromContext = _httpContextAccessor.HttpContext.Request.Cookies[_configSettings.Value.Globalization.CookieKey];

                // string country = "US";
                var CountryCode = "US";// GetCookieValue(country, cookieValueFromContext);

                var configuration = _getCurrentMarket.curretMarket(CountryCode).GetConfiguration().Orders;
                // var configuration = GlobalMarket.GetCurrentMarket(CountryCode).GetConfiguration().Orders;  //.GetCurrentMarket().GetConfiguration().Orders;)


                // // Create the request
                var request = new CreateCustomerRequest();
                request.InsertEnrollerTree = true;
                request.FirstName = model.FirstName;
                request.LastName = model.LastName;
                request.Phone = model.Phone;
                request.MobilePhone = model.MobilePhone;
                request.Email = model.Email;
                request.CanLogin = true;
                request.LoginName = model.LoginName;
                request.LoginPassword = model.LoginPassword;
                request.CustomerType = CustomerTypes.RetailCustomer;
                request.CustomerStatus = (int?)CustomerStatuses.Active;
                request.EntryDate = DateTime.Now; //DateTime.Now.ToCST();
                request.DefaultWarehouseID = configuration.WarehouseID;
                request.CurrencyCode = configuration.CurrencyCode;
                request.LanguageID = configuration.LanguageID;
                request.EnrollerID = 1; //model.EnrollerID1 == 0 ? 2 : model.EnrollerID1;
                request.MainCountry = CountryCode; //GlobalUtilities.GetSelectedCountryCode();
                // Create the customer
                var response = await _exigoApiContext.GetContext().CreateCustomerAsync(request); //createCustomerRequest(request);

                if (model.IsOptedIn)
                {
                    await _customerService.SendEmailVerification(response.CustomerID, request.Email);
                }
                //281021
                //var createCustomerRequest = _mapper.Map<CreateCustomerRequest>(model);

                //Create customer in Exigo service
                //await _authenticateService.CreateCustomer(createCustomerRequest);

                //Authenticate customer
                var result = await _authenticateService.SignInCustomer(new AuthenticateCustomerRequest { LoginName = model.LoginName, Password = model.LoginPassword });

                return Ok(new CustomerCreateResponse
                {
                    Email = model.Email,
                    LoginName = model.LoginName,
                    Phone = model.Phone,
                    Token = result.Token.ToString()
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Signin customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("SignInCustomer")]
        public async Task<IActionResult> SignInCustomer(CustomerSignInModel model)
        {
            try
            {
                var signinRequest = _mapper.Map<AuthenticateCustomerRequest>(model);

                //Signin customer in Exigo service
                return Ok(await _authenticateService.SignInCustomer(signinRequest));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Send forgot password email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("SendForgotPasswordEmail")]
        public async Task<IActionResult> SendForgotPasswordEmail(CustomerUpdateModel model)
        {
            try
            {
                var customerEmailRequest = _mapper.Map<CustomerUpdateRequest>(model);

                //Send email with Exigo service
                return Ok(await _authenticateService.SendForgotPasswordEmail(customerEmailRequest));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Update customer password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(CustomerUpdateModel model)
        {
            try
            {
                var customerUpdateRequest = _mapper.Map<CustomerUpdateRequest>(model);

                //Update customer password with Exigo service
                return Ok(await _authenticateService.UpdateCustomerPassword(customerUpdateRequest));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Validate customer email/username
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("ValidateCustomer")]
        public async Task<IActionResult> ValidateCustomer(CustomerValidationModel model)
        {
            try
            {
                var customerValidationRequest = _mapper.Map<CustomerValidationRequest>(model);

                //Validate username/email with Exigo service
                return Ok(await _authenticateService.IsEmailOrUsernameExists(customerValidationRequest));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion
    }
}
