using Dapper;
using Exigo.Api.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web.Http.Results;
using WinkNatural.Web.Common;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Common.Utils.Enum;
using WinkNatural.Web.Services.DTO;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Helpers;
using WinkNaturals.Models;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Setting.Interfaces;

namespace WinkNatural.Web.Services.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        //private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);

        private readonly IConfiguration _config;
        private readonly IOrderConfiguration _orderConfiguration;
        private readonly IExigoApiContext _exigoApiContext;
        private readonly ICustomerService _customerService;
        private readonly IShoppingService _shoppingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IOrderConfiguration AutoOrderConfiguration { get; set; }

        public EnrollmentService(IConfiguration config, IOrderConfiguration orderConfiguration, IExigoApiContext exigoApiContext, ICustomerService customerService, IShoppingService shoppingService, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _orderConfiguration = orderConfiguration;
            _exigoApiContext = exigoApiContext;
            _customerService = customerService;
            _shoppingService = shoppingService;
            _httpContextAccessor = httpContextAccessor;

        }
        public List<EnrollmentResponse> GetItems()
        {
            try
            {
                var apiItems = new List<EnrollmentResponse>();
                GetItemsRequest request = new GetItemsRequest
                {
                    //We will be requesting three items
                    ItemCodes = new string[3]
                };
                request.ItemCodes[0] = "SK-Q1KIT3-21";
                request.ItemCodes[1] = "SK-Q1KIT2-21";
                request.ItemCodes[2] = "SK-Q1KIT1-21";
                using (var context = DbConnection.Sql())
                {
                    apiItems = context.Query<EnrollmentResponse>(@"
                			    SELECT
	                                ItemID = i.ItemID,
	                                ItemCode = i.ItemCode,
	                                ItemDescription = 
		                                case 
			                                when i.IsGroupMaster = 1 then COALESCE(i.GroupDescription, il.ItemDescription, i.ItemDescription)
			                                when il.ItemDescription != '' then COALESCE(il.ItemDescription, i.ItemDescription)
							                else i.ItemDescription
		                                end,
	                                Weight = i.Weight,
	                                ItemTypeID = i.ItemTypeID,
	                                TinyImageUrl = i.TinyImageName,
	                                SmallImageUrl = i.SmallImageName,
	                                LargeImageUrl = i.LargeImageName,
	                                ShortDetail1 = COALESCE(il.ShortDetail, i.ShortDetail),
	                                ShortDetail2 = COALESCE(il.ShortDetail2, i.ShortDetail2),
	                                ShortDetail3 = COALESCE(il.ShortDetail3, i.ShortDetail3),
	                                ShortDetail4 = COALESCE(il.ShortDetail4, i.ShortDetail4),"
                       + (true
                         ? @"LongDetail1 = COALESCE(il.LongDetail, i.LongDetail),
	                                LongDetail2 = COALESCE(il.LongDetail2, i.LongDetail2),
	                                LongDetail3 = COALESCE(il.LongDetail3, i.LongDetail3),
	                                LongDetail4 = COALESCE(il.LongDetail4, i.LongDetail4),"
                         : string.Empty)
                       + @"IsVirtual = i.IsVirtual,
	                                AllowOnAutoOrder = i.AllowOnAutoOrder,
	                                IsGroupMaster = i.IsGroupMaster,
	                                IsDynamicKitMaster = cast(case when i.ItemTypeID = 2 then 1 else 0 end as bit),
	                                GroupMasterItemDescription = i.GroupDescription,
	                                GroupMembersDescription = i.GroupMembersDescription,
	                                Field1 = i.Field1,
	                                Field2 = i.Field2,
	                                Field3 = i.Field3,
	                                Field4 = i.Field4,
	                                Field5 = i.Field5,
	                                Field6 = i.Field6,
	                                Field7 = i.Field7,
	                                Field8 = i.Field8,
	                                Field9 = i.Field9,
	                                Field10 = i.Field10,
	                                OtherCheck1 = i.OtherCheck1,
	                                OtherCheck2 = i.OtherCheck2,
	                                OtherCheck3 = i.OtherCheck3,
	                                OtherCheck4 = i.OtherCheck4,
	                                OtherCheck5 = i.OtherCheck5,
	                                Price = ip.Price,
	                                CurrencyCode = ip.CurrencyCode,
	                                BV = ip.BusinessVolume,
	                                CV = ip.CommissionableVolume,
	                                OtherPrice1 = ip.Other1Price,
	                                OtherPrice2 = ip.Other2Price,
	                                OtherPrice3 = ip.Other3Price,
	                                OtherPrice4 = ip.Other4Price,
	                                OtherPrice5 = ip.Other5Price,
	                                OtherPrice6 = ip.Other6Price,
	                                OtherPrice7 = ip.Other7Price,
	                                OtherPrice8 = ip.Other8Price,
	                                OtherPrice9 = ip.Other9Price,
	                                OtherPrice10 = ip.Other10Price
                                FROM Items i
	                                INNER JOIN ItemPrices ip
		                                ON ip.ItemID = i.ItemID
		                                    AND ip.PriceTypeID = @priceTypeID
						                    AND ip.CurrencyCode = @currencyCode                                
	                                INNER JOIN ItemWarehouses iw
		                                ON iw.ItemID = i.ItemID
		                                    AND iw.WarehouseID = @warehouse
						            LEFT JOIN ItemLanguages il
		                                ON il.ItemID = i.ItemID
						                    AND il.LanguageID = @languageID
					            WHERE i.ItemCode in @itemCodes", new
                       {
                           warehouse = _orderConfiguration.WarehouseID,
                           currencyCode = _orderConfiguration.CurrencyCode,
                           languageID = _orderConfiguration.LanguageID,
                           priceTypeID = _orderConfiguration.PriceTypeID,
                           itemCodes = request.ItemCodes,
                       }).ToList();

                    return apiItems;
                }
            }
            catch (Exception ex)
            {
                var enrollmentResponse = new EnrollmentResponse { Success = false, ErrorMessage = ex.Message };
                return new List<EnrollmentResponse> { enrollmentResponse };
            }
        }
        public async Task<TransactionalResponse> SubmitCheckout(TransactionalRequestModel transactionRequest, int customerId)
        {
            int arraySize = 4;
            Exigo.Api.Client.TransactionalResponse response = new();
            Exigo.Api.Client.TransactionalRequest request = new()
            {
                TransactionRequests = new ITransactionMember[arraySize]
            };
            try
            {
                var hasOrder = transactionRequest.SetListItemRequest.Where(x => x.OrderType == ShoppingCartItemType.Order || x.OrderType == ShoppingCartItemType.EnrollmentPack).ToList();
                var hasAutoOrder = transactionRequest.SetListItemRequest.Where(x => x.OrderType == ShoppingCartItemType.AutoOrder || x.OrderType == ShoppingCartItemType.EnrollmentAutoOrderPack).ToList().Count > 0;
                var customertype = _exigoApiContext.GetContext(false).GetCustomersAsync(new GetCustomersRequest { CustomerID = customerId }).Result.Customers[0].CustomerType;
                var customerDetails = _exigoApiContext.GetContext(false).GetCustomersAsync(new GetCustomersRequest { CustomerID = customerId });
                var autoOrderPaymentType = AutoOrderPaymentType.PrimaryCreditCard;
                string ipaddress = string.Empty;
                    IPAddress iP = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
                    if (iP != null)
                    {
                        if (iP.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            iP = Dns.GetHostEntry(iP).AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
                        }
                        ipaddress = iP.ToString();
                    }
                    foreach (var itm in hasOrder)
                    {
                        if (itm.OtherCheck1)
                        {
                            itm.shippingPriceEachOverride = 0M;

                        }
                    }
                    CreateCustomerRequest createCustomerRequest = new()
                    {
                    LoginName = transactionRequest.CreateCustomerRequest.LoginName,//"abcd13",
                    LoginPassword = transactionRequest.CreateCustomerRequest.LoginPassword,//"abcd13",
                    FirstName = transactionRequest.CreateCustomerRequest.FirstName,//"abcd13",       
                    LastName = transactionRequest.CreateCustomerRequest.LastName,//"abcd13",          
                    Company = transactionRequest.CreateCustomerRequest.Company,//"ACME, Inc.",    
                    CustomerType = CustomerTypes.Distributor2,          
                    CustomerStatus = 1,       
                    Email = transactionRequest.CreateCustomerRequest.Email,//"joecool@me.com",
                    Phone = transactionRequest.CreateCustomerRequest.Phone,//"555-555-5555",
                    MainAddress1 = transactionRequest.CreateCustomerRequest.MainAddress1,//"123 Some Street",
                    MainCity = transactionRequest.CreateCustomerRequest.MainCity,//"Dallas",
                    MainState = transactionRequest.CreateCustomerRequest.MainState,//"TX",          
                    MainZip = transactionRequest.CreateCustomerRequest.MainZip,//"75207",
                    //Mailing Address
                    MailState = transactionRequest.CreateCustomerRequest.MainState,//"TX",            
                    MailCountry = transactionRequest.CreateCustomerRequest.MainCountry,//"USA",        
                    OtherState = transactionRequest.CreateCustomerRequest.OtherState,//"TX",         
                    OtherCountry = transactionRequest.CreateCustomerRequest.OtherCountry,//"USA",         
                    MiddleName = transactionRequest.CreateCustomerRequest.MiddleName,// "ABCd13",         
                    NameSuffix = transactionRequest.CreateCustomerRequest.NameSuffix,//"ABCd13",   
                    MainCountry= transactionRequest.CreateCustomerRequest.MainCountry,//"US",
                    MainCounty= transactionRequest.CreateCustomerRequest.MainCounty,//"US",
                    InsertEnrollerTree = true,
                    InsertUnilevelTree = true,
                    EnrollerID = 1,
                    SponsorID = 2,
                    DefaultWarehouseID = _orderConfiguration.WarehouseID,
                    CanLogin = true,
                    Notes = "Distributor was entered by Distributor 1. Created by the API Enrollment at " + _httpContextAccessor.HttpContext.Request.Host.Value + " on " + DateTime.Now.ToCST().ToString("dddd, MMMM d, yyyy h:mmtt") + " CST at IP " + ipaddress,
                    };
                    request.TransactionRequests[0] = createCustomerRequest;
                    ChargeCreditCardTokenRequest chargeCreditCardTokenRequest = new()
                    {
                        CreditCardToken = "41X111UAXYE31111",// transactionRequest.ChargeCreditCardTokenRequest.CreditCardToken,
                        BillingName = transactionRequest.ChargeCreditCardTokenRequest.BillingName,
                        BillingAddress = transactionRequest.ChargeCreditCardTokenRequest.BillingAddress,
                        BillingAddress2 = null,//transactionRequest.ChargeCreditCardTokenRequest.BillingAddress2,
                        BillingCity = transactionRequest.ChargeCreditCardTokenRequest.BillingCity,
                        BillingZip = transactionRequest.ChargeCreditCardTokenRequest.BillingZip,
                        ExpirationMonth = transactionRequest.ChargeCreditCardTokenRequest.ExpirationMonth,
                        ExpirationYear = transactionRequest.ChargeCreditCardTokenRequest.ExpirationYear,
                        BillingCountry = transactionRequest.ChargeCreditCardTokenRequest.BillingCountry,
                        BillingState = transactionRequest.ChargeCreditCardTokenRequest.BillingState,
                        MaxAmount = Math.Round((decimal)transactionRequest.ChargeCreditCardTokenRequest.MaxAmount, 2),
                        //OrderKey = "1",
                    };
                    request.TransactionRequests[1] = chargeCreditCardTokenRequest;

                    if (hasOrder.Count > 0)
                    {
                        CreateOrderRequest customerOrderRequest = new()
                        {
                            CustomerID = 0,
                            OrderStatus = Exigo.Api.Client.OrderStatusType.Incomplete,
                            OrderDate = DateTime.Now,
                            CurrencyCode = _orderConfiguration.CurrencyCode,
                            WarehouseID = _orderConfiguration.WarehouseID, // WarehouseID,
                            ShipMethodID = _orderConfiguration.DefaultShipMethodID, //ToDo
                            PriceType = _orderConfiguration.PriceTypeID,
                            FirstName = transactionRequest.CreateOrderRequest.FirstName,
                            LastName = transactionRequest.CreateOrderRequest.LastName,
                            Company = transactionRequest.CreateOrderRequest.Company,
                            Address1 = transactionRequest.CreateOrderRequest.Address1,
                            Address2 = transactionRequest.CreateOrderRequest.Address2,
                            Address3 = transactionRequest.CreateOrderRequest.Address3,
                            City = transactionRequest.CreateOrderRequest.City,
                            Zip = transactionRequest.CreateOrderRequest.Zip,
                            Country = transactionRequest.CreateOrderRequest.Country,
                            State = transactionRequest.CreateOrderRequest.State,
                            Email = transactionRequest.CreateOrderRequest.Email,
                            Phone = transactionRequest.CreateOrderRequest.Phone,
                            Notes = transactionRequest.CreateOrderRequest.Notes,
                            Other11 = null,
                            Other12 = null,
                            Other13 = null,
                            Other14 = null,
                            Other15 = null,
                            Other16 = null,
                            Other17 = "0.0",
                            Other18 = null,
                            Other19 = null,
                            Other20 = null,
                            OrderType = OrderType.ShoppingCart,
                            Details = transactionRequest.CreateOrderRequest.Details.ToArray(),
                        };
                        request.TransactionRequests[2] = customerOrderRequest;
                    }

                else if (hasAutoOrder)
                {
                    CreateAutoOrderRequest createAutoOrderRequest = new()
                    {

                        Frequency = FrequencyType.Weekly,
                        StartDate = DateTime.Today,
                        CurrencyCode = _orderConfiguration.CurrencyCode,
                        WarehouseID = _orderConfiguration.WarehouseID,
                        ShipMethodID = 8,//_orderConfiguration.DefaultShipMethodID,// transactionRequest.CreateAutoOrderRequest.ShipMethodID,
                        PriceType = _orderConfiguration.PriceTypeID,
                        PaymentType = autoOrderPaymentType,
                        OverwriteExistingAutoOrder = true,
                        Details = transactionRequest.CreateAutoOrderRequest.Details.ToArray(),
                    };

                    request.TransactionRequests[2] = createAutoOrderRequest;
                }

                SetAccountCreditCardTokenRequest setAccountCreditCardTokenRequest = new()
                {

                    CustomerID = 0,
                    CreditCardAccountType = AccountCreditCardType.Primary,
                    CreditCardToken = "41X111UAXYE31111",//transactionRequest.ChargeCreditCardTokenRequest.CreditCardToken,
                    ExpirationMonth = Convert.ToInt32(transactionRequest.ChargeCreditCardTokenRequest.ExpirationMonth),
                    ExpirationYear = transactionRequest.SetAccountCreditCardTokenRequest.ExpirationYear,
                    CreditCardType = 1,
                    UseMainAddress = true,
                    //latest added code
                    BillingName = transactionRequest.SetAccountCreditCardTokenRequest.BillingName,
                    BillingAddress = transactionRequest.SetAccountCreditCardTokenRequest.BillingAddress,
                    BillingAddress2 = transactionRequest.SetAccountCreditCardTokenRequest.BillingAddress2,
                    BillingCity = transactionRequest.SetAccountCreditCardTokenRequest.BillingCity,
                    BillingZip = transactionRequest.SetAccountCreditCardTokenRequest.BillingZip,
                    BillingCountry = transactionRequest.SetAccountCreditCardTokenRequest.BillingCountry,
                    BillingState = transactionRequest.SetAccountCreditCardTokenRequest.BillingState,
                };
                request.TransactionRequests[3] = setAccountCreditCardTokenRequest;

                request.TransactionRequests = request.TransactionRequests.Where(x => x != null).ToArray();

                //TransactionRequest
                response = await _exigoApiContext.GetContext(false).ProcessTransactionAsync(request);
                if (response.TransactionResponses.Length > 0)
                {
                    await _customerService.SendEmailVerification(customerId, customerDetails.Result.Customers[0].Email);
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return response;
        }
        public List<dynamic> GetDistributors(int customerId)
        {
            {
                // assemble a list of customers who match the search criteria
                var enrollerCollection = new List<SearchResult>();
                int[] customerTypes = { CustomerTypes.Distributor, CustomerTypes.Distributor2, CustomerTypes.Distributor3 };
                var nodeDataRecords = new List<dynamic>();

                if (customerId == null)
                {
                    using (var context = Common.Utils.DbConnection.Sql())
                    {
                        nodeDataRecords = context.Query(@"
                                SELECT
                                    cs.CustomerID, cs.FirstName, cs.LastName, cs.WebAlias,
                                    c.MainCity, c.MainState, c.MainCountry
                                FROM CustomerSites cs
                                INNER JOIN Customers c
                                ON cs.CustomerID = c.CustomerID
                                WHERE c.CustomerTypeID IN @customertypes
                                AND cs.CustomerID = @customerid
                        ", new
                        {
                            customertypes = customerTypes,
                            customerid = customerId
                        }).ToList();
                    }
                }
                else
                {
                    using (var context = Common.Utils.DbConnection.Sql())
                    {
                        nodeDataRecords = context.Query(@"
                                SELECT
                                    cs.CustomerID, cs.FirstName, cs.LastName, cs.WebAlias,
                                    c.MainCity, c.MainState, c.MainCountry
                                FROM CustomerSites cs
                                INNER JOIN Customers c
                                ON cs.CustomerID = c.CustomerID
                                WHERE c.CustomerTypeID IN @customertypes
                                AND (c.FirstName LIKE @queryValue OR c.LastName LIKE @queryvalue OR cs.FirstName LIKE @queryValue OR cs.LastName LIKE @queryValue)
                        ", new
                        {
                            customertypes = customerTypes,
                            queryValue = "%" + customerId + "%"
                        }).ToList();
                    }
                }

                if (nodeDataRecords.Count() > 0)
                {
                    foreach (var record in nodeDataRecords)
                    {
                        var node = new SearchResult();
                        node.CustomerID = record.CustomerID;
                        node.FirstName = record.FirstName;
                        node.LastName = record.LastName;
                        node.MainCity = record.MainCity;
                        node.MainState = record.MainState;
                        node.MainCountry = record.MainCountry;
                        node.WebAlias = record.WebAlias;
                        enrollerCollection.Add(node);
                    }
                }
                return nodeDataRecords;
            }
        }

      
        public class SearchResult
        {
            public int CustomerID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName
            {
                get { return this.FirstName + " " + this.LastName; }
            }
            public string AvatarURL { get; set; }
            public string WebAlias { get; set; }
            public string ReplicatedSiteUrl { get; set; }
            
            public string MainState { get; set; }
            public string MainCity { get; set; }
            public string MainCountry { get; set; }
        }
    }
}

