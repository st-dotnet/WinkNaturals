using Dapper;
using Exigo.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Customer;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.ExigoService;
using WinkNaturals.Infrastructure.Services.Utilities;
using WinkNaturals.Models;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;

namespace WinkNatural.Web.Services.Services
{
    public class AuthenticateService : IAuthenticateService
    {
        // private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);
        private readonly IConfiguration _config;
        private readonly ICustomerService _customerService;
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IOptions<ConfigSettings> _configSettings;
        #region constructor
        private List<CustomerCreateModel> _users;
        public AuthenticateService(IConfiguration config, ICustomerService customerService, IExigoApiContext exigoApiContext, IOptions<ConfigSettings> configSettings)
        {
            _config = config;
            _customerService = customerService;
            _exigoApiContext = exigoApiContext;
            _configSettings = configSettings;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Create customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CreateCustomerResponse> CreateCustomer(CreateCustomerRequest request)
        {
            try
            {
                var res = await _exigoApiContext.GetContext(false).CreateCustomerAsync(request);
                return res;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        /// <summary>
        /// Login customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomerCreateResponse> LoginCustomer(AuthenticateCustomerRequest req)
        {
            try
            {
                //Exigo service login request
                var result = await _exigoApiContext.GetContext(false).AuthenticateCustomerAsync(req);
                if (result.CustomerID == 0)
                {
                    return new CustomerCreateResponse { ErrorMessage = "User is not authenticated." };
                }
                // Get customer
                var customer = await _customerService.GetCustomer(result.CustomerID);
                // var token = GenerateJwtToken(result, customer.Customers[0].Email);
                var token = GenerateJwtToken(result, customer.Customers[0].Email);
                return new CustomerCreateResponse
                {
                    CustomerId = customer.Customers[0].CustomerID,
                    Email = customer.Customers[0].Email,
                    LoginName = customer.Customers[0].LoginName,
                    Phone = customer.Customers[0].Phone,
                    Token = token,
                    TypeOfCustomer = customer.Customers[0].CustomerType.ToString()
                };
            }
            catch (Exception ex)
            {
                return new CustomerCreateResponse { ErrorMessage = "Invalid UserName and Password " };
            }
        }
        /// <summary>
        /// Update customer password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomerUpdateResponse> UpdateCustomerPassword(CustomerUpdateRequest request)
        {
            try
            {
                var customerResult = true;
                if (request.CustomerId == 0) return new CustomerUpdateResponse { Success = false, ErrorMessage = "Some issues occurred during updating the customer!" };
                var customer = new CustomerResponse();
                try
                {
                    //get customer by customerid
                    customer = _customerService.GetCustomer(request.CustomerId).Result.Customers[0];
                }
                catch (Exception)
                {
                    customerResult = false;
                }

                if (!customerResult) return new CustomerUpdateResponse { Success = false, ErrorMessage = "Unable to find the customer!" };
                //Customer update password request
                var customerUpdateRequest = new UpdateCustomerRequest
                {
                    CustomerID = request.CustomerId,
                    LoginPassword = request.NewPassword
                ,
                    LoginName = customer.LoginName
                };
                var result = await _exigoApiContext.GetContext(false).UpdateCustomerAsync(customerUpdateRequest);
                return new CustomerUpdateResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new CustomerUpdateResponse { Success = false, ErrorMessage = "Error occurred during update the password!" };
            }
        }

        /// <summary>
        /// Send forgot password email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomerUpdateResponse> SendForgotPasswordEmail(CustomerUpdateRequest request)
        {
            try
            {
                //Get customer by login name
                var getCustomerRequest = new GetCustomersRequest { Email = request.Email };
                var customer = await _exigoApiContext.GetContext(false).GetCustomersAsync(getCustomerRequest);

                var body = $"To reset your password click this link! <a href={request.Url}/{customer.Customers[0].CustomerID}>Reset Password</a>";


                var sendEmail = await _exigoApiContext.GetContext(false).SendEmailAsync(new SendEmailRequest
                {
                    CustomerID = customer.Customers[0].CustomerID,
                    Body = body,
                    MailFrom = _configSettings.Value.EmailConfiguration.NoReplyEmail,
                    MailTo = request.Email,
                    Subject = $"{_configSettings.Value.EmailConfiguration.CompanyName} - Forgot Password"
                });
                return new CustomerUpdateResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new CustomerUpdateResponse { Success = false, ErrorMessage = "Sorry your email has not been found within our system" };
            }
        }

        /// <summary>
        /// Validate username/email
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> IsEmailOrUsernameExists(CustomerValidationRequest request)
        {
            try
            {
                //Check if Email is exists or not
                if (!string.IsNullOrEmpty(request.Email) && string.IsNullOrEmpty(request.Username))
                {
                    var customerEmailResult = await _exigoApiContext.GetContext(false).GetCustomersAsync(new GetCustomersRequest { Email = request.Email });
                    if (customerEmailResult.Customers.Length != 0) return true;
                }
                if (!string.IsNullOrEmpty(request.Username))//Check if username is exists or not
                {
                    var customerUsernameResult = await _exigoApiContext.GetContext(false).GetCustomersAsync(new GetCustomersRequest { LoginName = request.Username });
                    if (customerUsernameResult.Customers.Length != 0) return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region private methods

        //Get CST datetime
        private static DateTime GetCSTSateTime()
        {
            try
            {
                DateTime datetimeNow = DateTime.Now;
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                return TimeZoneInfo.ConvertTime(datetimeNow, cstZone);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        //Generate JWT token
        private string GenerateJwtToken(AuthenticateCustomerResponse customer, string email)
        {
            try
            {
                // generate token that is valid for 7 days
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configSettings.Value.JwtSettings.Key);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim("customerId", customer.CustomerID.ToString()),
                    new Claim("firstName", customer.FirstName),
                    new Claim("lastName", customer.LastName),
                    new Claim("email", email)

                }),
                    Expires = DateTime.UtcNow.AddDays(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };



                var token = tokenHandler.CreateToken(tokenDescriptor);

                //var userId = int.Parse(token.Claims.First(x => x.Type == "id").Value);

                //// attach user to context on successful jwt validation
                //context.Items["User"] = userService.GetById(userId);

                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public CustomerCreateResponse Authenticate(AuthenticateCustomerRequest model)
        {
            var user = _users.SingleOrDefault(x => x.LoginName == model.LoginName && x.LoginPassword == model.Password);

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new CustomerCreateResponse(user, token);
        }

        public IEnumerable<CustomerCreateModel> GetAll()
        {
            return _users;
        }

        public CustomerCreateModel GetById(int id)
        {
            return _users.FirstOrDefault(x => x.CustomerID == id);
        }
        private string generateJwtToken(CustomerCreateModel user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configSettings.Value.AppSettings.Secret); //_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.CustomerID.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<Address> SaveNewCustomerAddress(int customerId, Address address)
        {
            var addressesOnFile = GetCustomerAddresses(customerId).Where(c => c.IsComplete);

            // Do any of the addresses on file match the one we are using?
            // If not, save this address to the next available slot
            if (!addressesOnFile.Any(c => c.Equals(address)))
            {
                var saveAddress = false;
                var request = new UpdateCustomerRequest();
                request.CustomerID = customerId;

                // Main address
                if (!addressesOnFile.Any(c => c.AddressType == AddressType.Main))
                {
                    saveAddress = true;
                    address.AddressType = AddressType.Main;
                    request.MainAddress1 = address.Address1;
                    request.MainAddress2 = address.Address2;
                    request.MainCity = address.City;
                    request.MainState = address.State;
                    request.MainZip = address.Zip;
                    request.MainCountry = address.Country;
                }

                // Mailing address
                else if (!addressesOnFile.Any(c => c.AddressType == AddressType.Mailing))
                {
                    saveAddress = true;
                    address.AddressType = AddressType.Mailing;
                    request.MailAddress1 = address.Address1;
                    request.MailAddress2 = address.Address2;
                    request.MailCity = address.City;
                    request.MailState = address.State;
                    request.MailZip = address.Zip;
                    request.MailCountry = address.Country;
                }

                // Other address
                else
                {
                    saveAddress = true;
                    address.AddressType = AddressType.Other;
                    request.OtherAddress1 = address.Address1;
                    request.OtherAddress2 = address.Address2;
                    request.OtherCity = address.City;
                    request.OtherState = address.State;
                    request.OtherZip = address.Zip;
                    request.OtherCountry = address.Country;
                }

                if (saveAddress)
                {
                    await _exigoApiContext.GetContext(false).UpdateCustomerAsync(request);
                }
            }

            return address;
        }
        private List<Address> GetCustomerAddresses(int customerId)
        {
            var addresses = new List<Address>();
            try
            {
                using (var sql = Common.Utils.DbConnection.Sql())
                {
                    var model = sql.Query(@"
                            select 
                                c.FirstName,
                                c.LastName,
                                c.Email,
                                c.Phone,

                                c.MainAddress1,
                                c.MainAddress2,
                                c.MainCity,
                                c.MainState,
                                c.MainZip,
                                c.MainCountry,

                                c.MailAddress1,
                                c.MailAddress2,
                                c.MailCity,
                                c.MailState,
                                c.MailZip,
                                c.MailCountry,

                                c.OtherAddress1,
                                c.OtherAddress2,
                                c.OtherCity,
                                c.OtherState,
                                c.OtherZip,
                                c.OtherCountry

                            from Customers c
                            where c.CustomerID = @customerID
                            ", new { customerId }).FirstOrDefault();

                    addresses.Add(new ShippingAddress()
                    {
                        AddressType = AddressType.Main,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address1 = model.MainAddress1,
                        Address2 = model.MainAddress2,
                        City = model.MainCity,
                        State = model.MainState,
                        Zip = model.MainZip,
                        Country = model.MainCountry
                    });

                    addresses.Add(new ShippingAddress()
                    {
                        AddressType = AddressType.Mailing,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address1 = model.MailAddress1,
                        Address2 = model.MailAddress2,
                        City = model.MailCity,
                        State = model.MailState,
                        Zip = model.MailZip,
                        Country = model.MailCountry
                    });

                    addresses.Add(new ShippingAddress()
                    {
                        AddressType = AddressType.Other,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Address1 = model.OtherAddress1,
                        Address2 = model.OtherAddress2,
                        City = model.OtherCity,
                        State = model.OtherState,
                        Zip = model.OtherZip,
                        Country = model.OtherCountry
                    });
                }
            }
            catch
            {

            }
            return addresses;
        }
        
        public async Task<IAddress> ValidateAddress(IAddress address)
        {
            var validateAddresses = true;
            var isShippingAddress = address.CanBeParsedAs<ShippingAddress>();
            var shippingAddress = new ShippingAddress();
            var normalAddress = new Address();
           
            // Turn the address passed in into its correct type
            if (isShippingAddress)
            {
                shippingAddress = address as ShippingAddress;
            }
            else
            {
                normalAddress = address as Address;
            }

            // Ensure that only US addresses are attempted to be validated
            if (validateAddresses && address.Country == "US")
            {
                // Handle shipping addresses and regular addresses differently
                if (isShippingAddress)
                {
                    // Convert the shipping address to a normal address
                    var convertedAddress = new Address(shippingAddress);

                    // Verify that the newly converted address is valid
                    var verifyAddressResponse =  await _customerService.VerifyAddress(convertedAddress);   //VerifyAddress(convertedAddress);

                    // if the address can be validated, update and return the shipping address, otherwise return the original shipping address
                    if (verifyAddressResponse.IsValid)
                    {

                        var verifiedAddress = verifyAddressResponse.VerifiedAddress;
                        shippingAddress.AddressType = verifiedAddress.AddressType;
                        shippingAddress.Address1 = verifiedAddress.Address1;
                        shippingAddress.Address2 = verifiedAddress.Address2;
                        shippingAddress.City = verifiedAddress.City;
                        shippingAddress.State = verifiedAddress.State;
                        shippingAddress.Zip = verifiedAddress.Zip;
                        shippingAddress.Country = verifiedAddress.Country;

                        return shippingAddress;
                    }
                    else
                    {
                        return address;
                    }
                }
                else
                {
                    // Verify that the address is valid
                    var verifyAddressResponse = await _customerService.VerifyAddress(normalAddress);

                    // if the address can be validated, return the validated address, otherwise return the original address
                    if (verifyAddressResponse.IsValid)
                    {
                        return verifyAddressResponse.VerifiedAddress;
                        //return null;
                    }
                    else
                    {
                        return address;
                    }
                }
            }
            else
            {
                return address;
            }
        }

      

        #endregion
    }
}
