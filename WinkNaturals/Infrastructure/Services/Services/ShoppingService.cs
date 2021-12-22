using Dapper;
using Exigo.Api.Client;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Common;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNatural.Web.Services.Interfaces;
using WinkNatural.Web.Services.Utilities;
using WinkNaturals.Models;
using WinkNaturals.Models.ShipMethod;
using WinkNaturals.Models.Shopping.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces.PointAccount;
using WinkNaturals.Setting;
using WinkNaturals.Setting.Interfaces;
using AddressType = WinkNatural.Web.Services.DTO.Shopping.AddressType;
using ShippingAddress = WinkNatural.Web.Services.DTO.Shopping.ShippingAddress;

namespace WinkNatural.Web.Services.Services
{
    public class ShoppingService : IShoppingService
    {
        // private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IOptions<ConfigSettings> _config;
        
        public ShoppingService(IOptions<ConfigSettings> config, IExigoApiContext exigoApiContext)
        {
            _config = config;
            _exigoApiContext = exigoApiContext;
        }
        private readonly ICustomerAutoOreder _customerAuto;
        private readonly IOrderConfiguration _orderConfiguration;
    

        public ShoppingService(IOptions<ConfigSettings> config, IExigoApiContext exigoApiContext, ICustomerAutoOreder customerAuto,IOrderConfiguration orderConfiguration)
        {
            _config = config;
            _exigoApiContext = exigoApiContext;
            _customerAuto = customerAuto;
            _orderConfiguration = orderConfiguration;
          
        }

        /// <summary>
        /// GetItemCategory
        /// </summary>
        /// <param name="int categoryID, int pageSize, int pageIndex, string[] sizes = null, int sortBy = 0"></param>
        /// <returns>List of ItemCategoryResponse</returns>
        public List<ItemCategoryResponse> GetItemCategory(int webCategoryID)
        {

            webCategoryID = webCategoryID == 0 ? 1 : webCategoryID;
            var categories = new List<ItemCategoryResponse>();

            using (var context = DbConnection.Sql())
            {
                var data = context.Query<ItemCategoryResponse>(QueryUtility.itemCategoryList_Query, new
                {
                    webid = 1,
                    webcategoryid = webCategoryID
                }).ToList();

                categories = GetWebCategoriesRecursively(categories, data, webCategoryID);
            }

            return categories.OrderBy(c => c.SortOrder).ToList();
        }

        /// <summary>
        /// GetShopProducts
        /// </summary>
        /// <param name="int categoryID, int pageSize, int pageIndex, string[] sizes = null, int sortBy = 0"></param>
        /// <returns>List of ShopProductsResponse</returns>
        public List<ShopProductsResponse> GetShopProducts(int categoryID, int sortBy, int pageSize, int pageIndex, string[] sizes = null)
        {
            categoryID = categoryID == 0 ? 1 : categoryID;
            var categories = new List<ShopProductsResponse>();
            GetItemListRequest itemsRequest;
            var items = new List<ShopProductsResponse>();
            var newItems = new List<ShopProductsResponse>();
            itemsRequest = new GetItemListRequest
            {
                IncludeChildCategories = true,
                CategoryID = categoryID,
                SortBy = sortBy
            };
            items = GetItems(itemsRequest, false).OrderBy(c => c.SortOrder).ToList();
            return items;
        }

        public List<ShopProductsResponse> SearchProducts(string query)
        {
            var items = new List<ShopProductsResponse>();

            int CategoryID = 3;
            var webcategoryIDs = GetWebCategoriesRecursively(CategoryID).Select(c => c.WebCategoryID).Distinct().ToList();

            using (var context = Common.Utils.DbConnection.Sql())
            {
                var itemCodes = context.Query<string>(@"
				SELECT 
					i.ItemCode
				FROM Items i
				WHERE (i.ItemCode LIKE @queryValue OR i.ItemDescription LIKE @queryValue)
			", new
                {
                    queryValue = "%" + query + "%"
                }).ToList();

                var categoryItemCodes = context.Query<string>(@"
				SELECT
					i.ItemCode
				FROM WebCategoryItems w
					INNER JOIN Items i
						ON i.ItemID = w.ItemID
				WHERE w.WebCategoryID in @webcategoryids
			", new
                {
                    webcategoryids = webcategoryIDs
                }).Distinct().ToList();

                itemCodes = itemCodes.Where(c => categoryItemCodes.Contains(c)).ToList();


                var groupMasterItemIDs = context.Query<int>(@"
				SELECT
					i.ItemID	
				FROM Items i
				WHERE i.ItemCode in @itemcodes
					AND i.IsGroupMaster = 1
			", new
                {
                    itemcodes = itemCodes
                }).Distinct().ToList();

                if (groupMasterItemIDs.Any())
                {
                    // Get a distinct list of group members                        
                    var groupMemberItemCodes = context.Query<string>(@"
					SELECT
						i.ItemCode	
					FROM ItemGroupMembers g
						INNER JOIN Items i
							ON i.ItemID = g.ItemID
					WHERE g.MasterItemID in @groupmasteritemids
						AND g.MasterItemID <> g.ItemID
				", new
                    {
                        groupmasteritemids = groupMasterItemIDs
                    }).ToList();

                    itemCodes.AddRange(groupMemberItemCodes);
                }

                if (itemCodes.Any())
                {
                    GetItemListRequest itemsRequest;

                    itemsRequest = new GetItemListRequest
                    {
                        ItemCodes = itemCodes.ToArray()
                    };


                    items = GetItems(itemsRequest, false).OrderBy(c => c.SortOrder).ToList();

                    items = items.Where(c => c.IsGroupMaster == false).ToList();
                }

            }

            return items;
        }

       
        /// <summary>
        /// GetProductDetailById
        /// </summary>
        /// <param name="itemCode of string type"></param>
        /// <returns>Product Detail</returns>
        public ShopProductsResponse GetProductDetailById(string[] itemCodes)
        {
            //dynamic response;
            using (var context = Common.Utils.DbConnection.Sql())
            {
                var response = context.Query<ShopProductsResponse>(QueryUtility.getProductDetailById_Query, new
                {
                    warehouse = 1,
                    currencyCode = "usd",
                    languageID = 0,
                    priceTypeID = 1,
                    itemCodes = itemCodes 
                }).ToList();
                return response[0];
            }
        }
        /// <summary>
        /// GetProductDetailById
        /// </summary>
        /// <param name="itemCode of string type"></param>
        /// <returns>Product Detail</returns>
        public List<ShopProductsResponse> GetStaticProductDetailById(string[] itemCodes)
        {
            //dynamic response;
            using (var context = Common.Utils.DbConnection.Sql())
            {
                var response = context.Query<ShopProductsResponse>(QueryUtility.getProductDetailById_Query, new
                {
                    warehouse = 1,
                    currencyCode = "usd",
                    languageID = 0,
                    priceTypeID = 1,
                    itemCodes = itemCodes
                }).ToList();
                return response;
            }
        }
        /// <summary>
        /// GetProductImage
        /// </summary>
        /// <param name="imageName of string type"></param>
        /// <returns>byte[]</returns>
        public byte[] GetProductImage(string imageName)
        {
            try
            {
                object bytes;
                using (var context = Common.Utils.DbConnection.Sql())
                {
                    var query = QueryUtility.productImage_Query;

                    bytes = context.ExecuteScalar(query, new { Name = imageName });
                }
                var extension = Path.GetExtension(imageName).ToLower();
                string contentType = "image/jpeg";

                switch (extension)
                {
                    case ".gif":
                        contentType = "image/gif";
                        break;
                    case ".jpg":
                        contentType = "image/jpeg";
                        break;
                    case ".jpeg":
                        contentType = "image/png";
                        break;
                    case ".bmp":
                        contentType = "image/bmp";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                }

                return (byte[])bytes;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public ShopProductsResponse AddToCart(ShopProductsResponse shopProducts)
        {
            ShopProductsResponse productsResponse = new ShopProductsResponse();
            productsResponse = null;

            return productsResponse;
        }
        /// <summary>
        /// SubmitCheckout
        /// </summary>
        /// <param name="TransactionalRequestModel"></param>
        /// <returns>TransactionalResponse</returns>
        public async Task<Exigo.Api.Client.TransactionalResponse> SubmitCheckout(TransactionalRequestModel transactionRequest, int customerId,string email)
        {
            int arraySize = 5;
            Exigo.Api.Client.TransactionalResponse response = new();

            Exigo.Api.Client.TransactionalRequest request = new()
            {
                TransactionRequests = new ITransactionMember[arraySize]
            };
            try
            {
                var hasOrder = transactionRequest.SetListItemRequest.Where(x => x.OrderType == ShoppingCartItemType.Order).ToList().Count > 0;
                var hasAutoOrder = transactionRequest.SetListItemRequest.Where(x => x.OrderType == ShoppingCartItemType.AutoOrder).ToList().Count > 0;
                var customertype = _exigoApiContext.GetContext(false).GetCustomersAsync(new GetCustomersRequest { CustomerID = customerId }).Result.Customers[0].CustomerType;
                if (customertype == CustomerTypes.RetailCustomer)
                {
                    UpdateCustomerRequest updateCustomerRequest = new()
                    {
                        CustomerID = customerId,
                        CustomerType = CustomerTypes.RetailCustomer,
                        Field1 = hasAutoOrder ? "1" : string.Empty,
                        MainCountry = transactionRequest.CreateOrderRequest.Country,
                        MainState = transactionRequest.CreateOrderRequest.State,
                        OtherCountry = transactionRequest.CreateOrderRequest.Country,
                        OtherState = transactionRequest.CreateOrderRequest.State,
                        MailCountry = transactionRequest.CreateOrderRequest.Country,
                        MailState = transactionRequest.CreateOrderRequest.State,
                       
                    };
                    request.TransactionRequests[0] = updateCustomerRequest;
                }
                ChargeCreditCardTokenRequest chargeCreditCardTokenRequest = new()
                {
                    CreditCardToken = transactionRequest.ChargeCreditCardTokenRequest.CreditCardToken,//"41X111UAXYE31111",
                    BillingName = transactionRequest.ChargeCreditCardTokenRequest.BillingName,
                    BillingAddress = transactionRequest.ChargeCreditCardTokenRequest.BillingAddress,
                    BillingAddress2 = transactionRequest.ChargeCreditCardTokenRequest.BillingAddress2,
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
                if (hasOrder)
                {
                    CreateOrderRequest customerOrderRequest = new()
                    {
                        CustomerID = customerId,
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
                        Email = email,
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
                if (hasAutoOrder)
                {
                    CreateAutoOrderRequest createAutoOrderRequest = new()
                    {
                        CustomerID = customerId,
                        Frequency = FrequencyType.Weekly,
                        StartDate = DateTime.Today,
                        StopDate = DateTime.Today,               //Leave null if there is no stop date.
                        SpecificDayInterval = 1,    //To be used with Frequency Type SpecificDays
                        CurrencyCode = _orderConfiguration.CurrencyCode,
                        WarehouseID = _orderConfiguration.WarehouseID,            //Unique location for orders
                        ShipMethodID = _orderConfiguration.DefaultShipMethodID,
                        PriceType = _orderConfiguration.PriceTypeID,              //Controls which price band to use
                        PaymentType = AutoOrderPaymentType.PrimaryCreditCard,
                        ProcessType = AutoOrderProcessType.AlwaysProcess,
                        FirstName = transactionRequest.CreateAutoOrderRequest.FirstName,
                        LastName = transactionRequest.CreateAutoOrderRequest.LastName,
                        Company = transactionRequest.CreateAutoOrderRequest.Company,
                        Address1 = transactionRequest.CreateAutoOrderRequest.Address1,
                        Address2 = transactionRequest.CreateAutoOrderRequest.Address2,
                        Address3 = transactionRequest.CreateAutoOrderRequest.Address3,
                        City = transactionRequest.CreateAutoOrderRequest.City,
                        Zip = transactionRequest.CreateAutoOrderRequest.Zip,
                        County =transactionRequest.CreateAutoOrderRequest.County,
                        Email = transactionRequest.CreateAutoOrderRequest.Email,
                        Phone = transactionRequest.CreateAutoOrderRequest.Phone,
                        Notes = transactionRequest.CreateAutoOrderRequest.Notes,
                        Details = transactionRequest.CreateAutoOrderRequest.Details.ToArray(),
                        Country = transactionRequest.CreateAutoOrderRequest.Country,
                        State = transactionRequest.CreateAutoOrderRequest.State,
                    };
                    request.TransactionRequests[3] = createAutoOrderRequest;
                }

                SetAccountCreditCardTokenRequest setAccountCreditCardTokenRequest = new()
                {
                    CustomerID = customerId,
                    CreditCardAccountType = AccountCreditCardType.Primary,
                    CreditCardToken = transactionRequest.ChargeCreditCardTokenRequest.CreditCardToken,//"41X111UAXYE31111",//
                    ExpirationMonth = Convert.ToInt32(transactionRequest.SetAccountCreditCardTokenRequest.ExpirationMonth),
                    ExpirationYear = transactionRequest.SetAccountCreditCardTokenRequest.ExpirationYear,
                    CreditCardType = 1,
                    UseMainAddress = false,
                    //latest added code
                    BillingName = transactionRequest.SetAccountCreditCardTokenRequest.BillingName,
                    BillingAddress = transactionRequest.SetAccountCreditCardTokenRequest.BillingAddress,
                    BillingAddress2 = transactionRequest.SetAccountCreditCardTokenRequest.BillingAddress2,
                    BillingCity = transactionRequest.SetAccountCreditCardTokenRequest.BillingCity,
                    BillingZip = transactionRequest.SetAccountCreditCardTokenRequest.BillingZip,
                    BillingCountry = transactionRequest.SetAccountCreditCardTokenRequest.BillingCountry,
                    BillingState = transactionRequest.SetAccountCreditCardTokenRequest.BillingState,
                };
                request.TransactionRequests[4] = setAccountCreditCardTokenRequest;

                request.TransactionRequests = request.TransactionRequests.Where(x => x != null).ToArray();

               // arraySize = Convert.ToInt32(request.TransactionRequests);
                //TransactionRequest
                response = await _exigoApiContext.GetContext(false).ProcessTransactionAsync(request);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return response;
        }

       
        /// <summary>
        /// CalculateOrder
        /// </summary>
        /// <param name="CalculateOrderRequest"></param>
        /// <returns>CalculateOrderResponse</returns>
        public async Task<CalculateOrderResponse> CalculateOrder(CalculateOrderRequest request)
        {
            var res = new CalculateOrderResponse();
            try
            {
                var req = new CalculateOrderRequest
                {
                    CustomerID = request.CustomerID,
                    WarehouseID = request.WarehouseID,
                    CurrencyCode = request.CurrencyCode,
                    PriceType = request.PriceType,
                    ShipMethodID = request.ShipMethodID,
                    City = request.City,
                    State = request.State,
                    Zip = request.Zip,
                    Country = request.Country,
                    Address1 = request.Address1,
                    Address2 = request.Address2,
                    Address3 = " ",
                    // CustomerID = request.CustomerID.Value,
                    OrderType = request.OrderType,
                    //County=calculateOrder.

                    //CurrencyCode = "usd", 
                    //WarehouseID = 1,            //Unique location for orders 
                    //ShipMethodID = 8, 
                    //PriceType = 1,              //Controls which price band to use 
                    //Address1 = "Some Address", 
                    //Address2 = "Some Address",  
                    //Address3 = "Some Address", 
                    //City = "Dallas", 
                    //State = "TX", 
                    //Zip = "1", 
                    //Country = "US",

                    //County = "1",

                    //CustomerID = 103082,             //Unique numeric identifier for a customer record.

                    //OrderType = Exigo.Api.Client.OrderType.Default
                };

                //Add Details

                var details = new List<Exigo.Api.Client.OrderDetailRequest>();
                if (request.Details.Count() > 0)
                {
                    foreach (var item in request.Details)
                    {
                        var detail = new Exigo.Api.Client.OrderDetailRequest();
                        detail.ItemCode = item.ItemCode;
                        detail.Quantity = item.Quantity;
                        details.Add(detail);
                    }
                }

                //Now attach the list to the request

                req.Details = details.ToArray();

                req.ReturnShipMethods = true;

                req.PartyID = 1;

                req.CustomerKey = request.CustomerKey;//Unique alpha numeric identifier for customer record. Exeption will occur if CustomerID & CustomerKey are provided.

                res = await _exigoApiContext.GetContext(false).CalculateOrderAsync(req);

                var Details = res.Details;
                var Subtotal = res.SubTotal;
                var Shipping = res.ShippingTotal;
                var Tax = res.TaxTotal;
                var Discount = res.DiscountTotal;
                var Total = res.Total;


                // Assemble the ship methods, if requested
                //if (request.ReturnShipMethods)
                //{
                //    var shipMethods = new List<ShipMethod>();
                //    if (apiresponse.ShipMethods != null && apiresponse.ShipMethods.Length > 0)
                //    {
                //        var webShipMethods = new List<ShipMethod>();

                //        using (var context = Common.Utils.DbConnection.Sql())
                //        {
                //            webShipMethods = context.Query<ShipMethod>(@"
                //                                            SELECT 
                //                                                [ShipMethodID]
                //                                                ,[ShipMethodDescription]
                //                                                ,[WarehouseID]
                //                                                ,[ShipCarrierID]
                //                                                ,[DisplayOnWeb]
                //                                            FROM [dbo].[ShipMethods]
                //                                            WHERE DisplayOnWeb = 1 
                //                                                AND WarehouseID = @wid",
                //                                 new
                //                                 {
                //                                     wid = request.Configuration.WarehouseID
                //                                 }).ToList();
                //        }


                //        if (webShipMethods.Count() > 0)
                //        {
                //            var webShipMethodIds = webShipMethods.Select(s => s.ShipMethodID);
                //            foreach (var shipMethod in apiresponse.ShipMethods.Where(x => webShipMethodIds.Contains(x.ShipMethodID)))
                //            {
                //                shipMethods.Add(shipMethod as ShipMethod);
                //            }

                //            if (shipMethods.Any())
                //            {
                //                // Ensure that at least one ship method is selected
                //                var shipMethodID = (request.ShipMethodID != 0) ? request.ShipMethodID : request.Configuration.DefaultShipMethodID;
                //                if (shipMethods.Any(c => c.ShipMethodID == shipMethodID))
                //                {
                //                    shipMethods.First(c => c.ShipMethodID == shipMethodID).Selected = true;
                //                }
                //                else
                //                {
                //                    shipMethods.First().Selected = true;
                //                }
                //            }
                //        }
                //        else
                //        {
                //            throw new Exception("Error: You need at least one Ship Method set up as DisplayOnWeb.");
                //        }
                //    }
                //    //result.ShipMethods = shipMethods.AsEnumerable();
                //}


            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        /// Create Order
        /// </summary>
        /// <param name="CreateOrderRequest"></param>
        /// <returns>CreateOrderResponse</returns>
        public async Task<CreateOrderResponse> CreateOrder(CreateOrderRequest orderRequest)
        {
            var res = new CreateOrderResponse();
            try
            {
                //var res = new CreateOrderDetailResponse();
                var req = new CreateOrderRequest();

                req.CustomerID = orderRequest.CustomerID;             //Unique numeric identifier for a customer record.

                req.OrderStatus = orderRequest.OrderStatus;           // OrderStatusType.Incomplete; 

                req.OrderDate = DateTime.Today;

                req.CurrencyCode = orderRequest.CurrencyCode;         // "1";

                req.WarehouseID = orderRequest.WarehouseID;                 //1;            //Unique location for orders

                req.ShipMethodID = orderRequest.ShipMethodID;             //1;

                req.PriceType = orderRequest.PriceType;                    //1;              //Controls which price band to use

                req.FirstName = orderRequest.FirstName; //"1";

                req.LastName = orderRequest.LastName; //"1";

                req.Company = orderRequest.Company; //"1";

                req.Address1 = orderRequest.Address1;  // "1";

                req.Address2 = orderRequest.Address2;  // "1";

                req.Address3 = orderRequest.Address3; // "1";

                req.City = orderRequest.City;  // "1";

                req.Zip = orderRequest.Zip;  // "1";

                req.County = orderRequest.Country; //"1";
                req.State = orderRequest.State;

                req.Email = orderRequest.Email; //"1";

                req.Phone = orderRequest.Phone; //"1";

                req.Notes = orderRequest.Notes; //"1";

                req.Other11 = orderRequest.Other11; //"1";

                req.Other12 = orderRequest.Other12;// //"1";

                req.Other13 = orderRequest.Other13; //"1";

                req.Other14 = orderRequest.Other14; //"1";

                req.Other15 = orderRequest.Other15; //"1";

                req.Other16 = orderRequest.Other16;// "1";

                req.Other17 = orderRequest.Other17; //"1";

                req.Other18 = orderRequest.Other18; //"1";

                req.Other19 = orderRequest.Other19;// "1";

                req.Other20 = orderRequest.Other20; //"1";

                req.OrderType = orderRequest.OrderType;  //OrderType.Default;

                req.TransferVolumeToID = orderRequest.TransferVolumeToID; // 1;     //Only pass in if you want volume to goto another CustomerID

                req.ReturnOrderID = orderRequest.ReturnOrderID;  //1;          //Unique numeric identifier for return order record.

                req.OverwriteExistingOrder = true;

                req.ExistingOrderID = orderRequest.ExistingOrderID; //1;        //Unique numeric identifier for existing order record.

                //req.PartyID = orderRequest.PartyID;//1;



                //Add Details

                var details = new List<OrderDetailRequest>();
                if (orderRequest.Details.Count() > 0)
                {
                    foreach (var item in orderRequest.Details)
                    {
                        var detail = new OrderDetailRequest();
                        detail.ItemCode = item.ItemCode;
                        detail.Quantity = detail.Quantity;

                        details.Add(detail);
                    }
                }


                //var detail1 = new OrderDetailRequest();

                //detail1.ItemCode = "1";

                //detail1.Quantity = 1;

                //details.Add(detail1);



                //var detail2 = new OrderDetailRequest();

                //detail2.ItemCode = "2";

                //detail2.Quantity = 2;

                //details.Add(detail2);



                //Now attach the list to the request

                req.Details = details.ToArray();

                req.SuppressPackSlipPrice = true;

                //req.TransferVolumeToKey = orderRequest.TransferVolumeToKey;  //"1";  //Only pass in if you want volume to goto another Customer Key

                //req.ReturnOrderKey = orderRequest.ReturnOrderKey; //"1";       //Unique alpha numeric identifier for customer record. Exeption will occur if ReturnOrderID & ReturnOrderKey are provided.

                //req.ExistingOrderKey = orderRequest.ExistingOrderKey;// "1";     //Unique alpha numeric identifier for existing order record. Exeption will occur if ExistingOrderID & ExistingOrderKey are provided.

                //req.CustomerKey = orderRequest.CustomerKey;  //"1";          //Unique alpha numeric identifier for customer record. Exeption will occur if OrderID & OrderKey are provided.



                //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).CreateOrderAsync(req);



            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// CreateOrderImport
        /// </summary>
        /// <param name="CreateOrderImportRequest"></param>
        /// <returns>CreateOrderImportResponse</returns>
        public async Task<CreateOrderImportResponse> CreateOrderImport(CreateOrderImportRequest createOrderImport)
        {
            var res = new CreateOrderImportResponse();
            try
            {
                var req = new CreateOrderImportRequest();
                req.CustomerID = createOrderImport.CustomerID;      //1;             //Unique numeric identifier for customer record.
                req.FirstName = createOrderImport.FirstName;
                req.LastName = createOrderImport.LastName;
                req.Company = createOrderImport.Company;
                req.Phone = createOrderImport.Phone;
                req.Email = createOrderImport.Email;
                req.ShipMethodID = createOrderImport.ShipMethodID;
                req.Address1 = createOrderImport.Address1;
                req.Address2 = createOrderImport.Address2;
                req.Address3 = createOrderImport.Address3;
                req.City = createOrderImport.City;
                req.State = createOrderImport.State;
                req.Zip = createOrderImport.Zip;
                req.Country = createOrderImport.Country;
                req.County = createOrderImport.County;
                req.Notes = createOrderImport.Notes;
                req.WarehouseID = createOrderImport.WarehouseID;            //Unique location for orders
                req.CurrencyCode = createOrderImport.CurrencyCode;
                req.ShippingStateTax = createOrderImport.ShippingStateTax;
                req.ShippingFedTax = createOrderImport.ShippingFedTax;
                req.ShippingCountyLocalTax = createOrderImport.ShippingCountyLocalTax;
                req.ShippingCountyTax = createOrderImport.ShippingCountyTax;
                req.ShippingCityLocalTax = createOrderImport.ShippingCityLocalTax;
                req.ShippingCityTax = createOrderImport.ShippingCityTax;
                req.Shipping = createOrderImport.Shipping;
                req.PriceType = createOrderImport.PriceType;              //Controls which price band to use
                req.OrderStatus = Exigo.Api.Client.OrderStatusType.Incomplete;
                req.OrderDate = DateTime.Today;
                req.Other11 = createOrderImport.Other11;
                req.Other12 = createOrderImport.Other12;
                req.Other13 = createOrderImport.Other13;
                req.Other14 = createOrderImport.Other14;
                req.Other15 = createOrderImport.Other15;
                req.Other16 = createOrderImport.Other16;
                req.Other17 = createOrderImport.Other17;
                req.Other18 = createOrderImport.Other18;
                req.Other19 = createOrderImport.Other19;
                req.Other20 = createOrderImport.Other20;
                req.OrderType = createOrderImport.OrderType;

                var orderDetails = new List<OrderImportDetail>();
                if (createOrderImport.OrderDetails.Count() > 0)
                {
                    foreach (var item in createOrderImport.OrderDetails)
                    {
                        var orderDetail = new OrderImportDetail();
                        orderDetail.ItemCode = item.ItemCode;
                        orderDetail.Qty = item.Qty;
                        //orderDetail.ParentItemCode = item.ParentItemCode;
                        //orderDetail.Description = item.Description;
                        //orderDetail.WeightEach = item.WeightEach;
                        //orderDetail.CountyLocalTax = item.CountyLocalTax;
                        //orderDetail.CountyTax = item.CountyTax;
                        //orderDetail.CityTax = item.CityTax;
                        //orderDetail.StateTax = item.StateTax;
                        //orderDetail.FedTax = item.FedTax;
                        //orderDetail.TaxablePriceEach = item.TaxablePriceEach;
                        //orderDetail.CVEach = item.CVEach;
                        //orderDetail.BVEach = item.BVEach;
                        //orderDetail.PriceEach = item.PriceEach;
                        //orderDetail.Other10Each = item.Other10Each;
                        //orderDetail.Other9Each = item.Other9Each;
                        //orderDetail.Other8Each = item.Other8Each;
                        //orderDetail.Other7Each = item.Other7Each;
                        //orderDetail.Other6Each = item.Other6Each;
                        //orderDetail.Other5Each = item.Other5Each;
                        //orderDetail.Other4Each = item.Other4Each;
                        //orderDetail.Other3Each = item.Other3Each;
                        //orderDetail.Other2Each = item.Other2Each;
                        //orderDetail.Other1Each = item.Other1Each;
                        //orderDetail.Reference1 = item.Reference1;
                        orderDetails.Add(orderDetail);
                    }
                }

                req.OrderDetails = orderDetails.ToArray();
                // req.PartyID = createOrderImport.PartyID;
                //req.ManualOrderKey = createOrderImport.ManualOrderKey;       //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                //req.ReturnOrderKey = createOrderImport.ReturnOrderKey;       //Unique alpha numeric identifier for return order record. Exeption will occur if ReturnOrderID & ReturnOrderKey are provided.
                //req.CustomerKey = createOrderImport.CustomerKey;          //Unique alpha numeric identifier for customer record. Exeption will occur if CustomerID & CustomerKey are provided.
                //req.OverwriteExistingOrder = createOrderImport.OverwriteExistingOrder ? createOrderImport.OverwriteExistingOrder : true;
                //req.ExistingOrderID = createOrderImport.ExistingOrderID;        //Unique numeric identifier for existing order record.
                //req.ExistingOrderKey = createOrderImport.ExistingOrderKey;     //Unique alpha numeric identifier for existing order record. Exeption will occur if ExistingOrderID & ExistingOrderKey are provided.
                req.IsCommissionable = createOrderImport.IsCommissionable ? createOrderImport.IsCommissionable : true;

                //Send Request to Server and Get Response
                res = await _exigoApiContext.GetContext(false).CreateOrderImportAsync(req);


            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        /// UpdateOrder
        /// </summary>
        /// <param name="UpdateOrderRequest"></param>
        /// <returns>UpdateOrderResponse</returns>
        public async Task<UpdateOrderResponse> UpdateOrder(UpdateOrderRequest updateOrderRequest)
        {
            var res = new UpdateOrderResponse();
            try
            {
                var req = new UpdateOrderRequest();
                //req.OrderID = updateOrderRequest.OrderID;                //Unique numeric identifier for order record.
                req.OrderDate = DateTime.Today;
                req.DeclineCount = updateOrderRequest.DeclineCount;
                req.OrderTy = updateOrderRequest.OrderTy;
                req.OrderStatus = updateOrderRequest.OrderStatus;
                req.PriceTy = updateOrderRequest.PriceTy;                //Controls which price band to use
                req.Total = updateOrderRequest.Total;
                req.SubTotal = updateOrderRequest.SubTotal;
                req.Shipping = updateOrderRequest.Shipping;
                req.OrderTax = updateOrderRequest.OrderTax;
                req.ShippingTax = updateOrderRequest.ShippingTax;
                req.FedShippingTax = updateOrderRequest.FedShippingTax;
                req.StateShippingTax = updateOrderRequest.StateShippingTax;
                req.CityShippingTax = updateOrderRequest.CityShippingTax;
                req.CityLocalShippingTax = updateOrderRequest.CityLocalShippingTax;
                req.CountyShippingTax = updateOrderRequest.CountyShippingTax;
                req.CountyLocalShippingTax = updateOrderRequest.CountyLocalShippingTax;
                req.ManualTaxRate = updateOrderRequest.ManualTaxRate;
                req.TotalTax = updateOrderRequest.TotalTax;
                req.CurrencyCode = updateOrderRequest.CurrencyCode;
                req.PaymentMethod = updateOrderRequest.PaymentMethod;
                req.WarehouseID = updateOrderRequest.WarehouseID;            //Unique location for orders
                req.BatchID = updateOrderRequest.BatchID;
                req.PreviousBalance = updateOrderRequest.PreviousBalance;
                req.OverrideShipping = updateOrderRequest.OverrideShipping;
                req.OverrideTax = updateOrderRequest.OverrideTax;
                req.BusinessVolume = updateOrderRequest.BusinessVolume;         //Price amount
                req.CommissionableVolume = updateOrderRequest.CommissionableVolume;   //Price amount
                req.Discount = updateOrderRequest.Discount;
                req.DiscountPercent = updateOrderRequest.DiscountPercent;
                req.Weight = updateOrderRequest.Weight;
                req.Sourcety = updateOrderRequest.Sourcety;
                req.Notes = updateOrderRequest.Notes;
                req.Usr = updateOrderRequest.Usr;
                req.FirstName = updateOrderRequest.FirstName;
                req.LastName = updateOrderRequest.LastName;
                req.Company = updateOrderRequest.Company;
                req.Address1 = updateOrderRequest.Address1;
                req.Address2 = updateOrderRequest.Address2;
                req.City = updateOrderRequest.City;
                req.Zip = updateOrderRequest.Zip;
                req.Email = updateOrderRequest.Email;
                req.Phone = updateOrderRequest.Phone;
                req.SuppressPackSlipPrice = updateOrderRequest.SuppressPackSlipPrice;
                req.ShipMethodID = updateOrderRequest.ShipMethodID;
                req.AutoOrderID = updateOrderRequest.AutoOrderID;
                req.CreatedBy = updateOrderRequest.CreatedBy;
                req.ReturnOrderID = updateOrderRequest.ReturnOrderID;          //Unique numeric identifier for transfer to order record.
                req.OrderRankID = updateOrderRequest.OrderRankID;
                req.OrderPayRankID = updateOrderRequest.OrderPayRankID;
                req.AddressIsVerified = updateOrderRequest.AddressIsVerified;
                req.County = updateOrderRequest.County;
                req.IsRMA = updateOrderRequest.IsRMA;
                req.BackOrderFromID = updateOrderRequest.BackOrderFromID;        //Unique numeric identifier for transfer to order record.
                req.CreditsEarned = updateOrderRequest.CreditsEarned;
                req.TotalFedTax = updateOrderRequest.TotalFedTax;
                req.TotalStateTax = updateOrderRequest.TotalStateTax;
                req.ManualShippingTax = updateOrderRequest.ManualShippingTax;
                req.ReplacementOrderID = updateOrderRequest.ReplacementOrderID;     //Unique numeric identifier for transfer to order record.
                req.LockedDate = DateTime.Today;
                req.CommissionedDate = DateTime.Today;
                req.Flag1 = updateOrderRequest.Flag1;
                req.Flag2 = updateOrderRequest.Flag2;
                req.Flag3 = updateOrderRequest.Flag3;
                req.OriginalWarehouseID = updateOrderRequest.OriginalWarehouseID;
                req.PickupName = updateOrderRequest.PickupName;
                req.TransferToID = updateOrderRequest.TransferToID;           //Unique numeric identifier for transfer to order record.
                req.IsCommissionable = updateOrderRequest.IsCommissionable;
                req.FulfilledBy = updateOrderRequest.FulfilledBy;
                req.CreditApplied = updateOrderRequest.CreditApplied;
                req.ShippedDate = DateTime.Today;
                req.TaxLockDate = DateTime.Today;
                req.TotalTaxable = updateOrderRequest.TotalTaxable;
                req.ReturnCategoryID = updateOrderRequest.ReturnCategoryID;
                req.ReplacementCategoryID = updateOrderRequest.ReplacementCategoryID;
                req.CalculatedShipping = updateOrderRequest.CalculatedShipping;
                req.HandlingFee = updateOrderRequest.HandlingFee;
                req.OrderProcessTy = updateOrderRequest.OrderProcessTy;
                req.ActualCarrier = updateOrderRequest.ActualCarrier;
                req.ParentOrderID = updateOrderRequest.ParentOrderID;          //Unique numeric identifier for transfer to order record.
                req.CustomerTy = updateOrderRequest.CustomerTy;             //Classification of a customer as defined by the company
                req.Reference = updateOrderRequest.Reference;
                req.MiddleName = updateOrderRequest.MiddleName;
                req.NameSuffix = updateOrderRequest.NameSuffix;
                req.Address3 = updateOrderRequest.Address3;
                req.PartyID = updateOrderRequest.PartyID;
                req.TrackingNumber1 = updateOrderRequest.TrackingNumber1;
                req.TrackingNumber2 = updateOrderRequest.TrackingNumber2;
                req.TrackingNumber3 = updateOrderRequest.TrackingNumber3;
                req.TrackingNumber4 = updateOrderRequest.TrackingNumber4;
                req.TrackingNumber5 = updateOrderRequest.TrackingNumber5;
                req.WebCarrierID = OrderShipCarrier.FedEx;
                req.WebCarrierID2 = OrderShipCarrier.FedEx;
                req.WebCarrierID3 = OrderShipCarrier.FedEx;
                req.WebCarrierID4 = OrderShipCarrier.FedEx;
                req.WebCarrierID5 = OrderShipCarrier.FedEx;
                req.Other11 = updateOrderRequest.Other11;
                req.Other12 = updateOrderRequest.Other12;
                req.Other13 = updateOrderRequest.Other13;
                req.Other14 = updateOrderRequest.Other14;
                req.Other15 = updateOrderRequest.Other15;
                req.Other16 = updateOrderRequest.Other16;
                req.Other17 = updateOrderRequest.Other17;
                req.Other18 = updateOrderRequest.Other18;
                req.Other19 = updateOrderRequest.Other19;
                req.Other20 = updateOrderRequest.Other20;
                //req.TransferToKey = updateOrderRequest.TransferToKey;        //Unique alpha numeric identifier for transfer to order record. Exeption will occur if TransferToID & TransferToKey are provided.
                req.OrderKey = updateOrderRequest.OrderKey; //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                                                            //req.ReturnOrderKey = updateOrderRequest.ReturnOrderKey;       //Unique alpha numeric identifier for transfer to order record. Exeption will occur if ReturnOrderID & ReturnOrderKey are provided.
                                                            // req.ReplacementOrderKey = updateOrderRequest.ReplacementOrderKey;  //Unique alpha numeric identifier for transfer to order record. Exeption will occur if ReplacementOrderID & ReplacementOrderKey are provided.
                                                            //req.ParentOrderKey = updateOrderRequest.ParentOrderKey;       //Unique alpha numeric identifier for transfer to order record. Exeption will occur if ParentOrderID & ParentOrderKey are provided.
                req.BackOrderFromKey = updateOrderRequest.BackOrderFromKey;
                res = await _exigoApiContext.GetContext(false).UpdateOrderAsync(req);//
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }


            return res;
        }

        /// <summary>
        /// Update the status of an order
        /// </summary>
        /// <param name="ChangeOrderStatusRequest"></param>
        /// <returns>ChangeOrderStatusResponse</returns>
        public async Task<ChangeOrderStatusResponse> ChangeOrderStatus(ChangeOrderStatusRequest changeOrderStatusRequest)
        {
            var res = new ChangeOrderStatusResponse();
            try
            {
                var req = new ChangeOrderStatusRequest();
                req.OrderStatus = changeOrderStatusRequest.OrderStatus;
                req.OrderKey = changeOrderStatusRequest.OrderKey;
                res = await _exigoApiContext.GetContext(false).ChangeOrderStatusAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        ///ChangeOrderStatusBatch
        /// </summary>
        /// <param name="ChangeOrderStatusBatchRequest"></param>
        /// <returns>ChangeOrderStatusBatchResponse</returns>
        public async Task<ChangeOrderStatusBatchResponse> ChangeOrderStatusBatch(ChangeOrderStatusBatchRequest changeOrderStatusBatchRequest)
        {
            var res = new ChangeOrderStatusBatchResponse();
            try
            {
                var req = new ChangeOrderStatusBatchRequest();
                req.OrderStatus = changeOrderStatusBatchRequest.OrderStatus;

                var details = new List<OrderBatchDetailRequest>();
                if (changeOrderStatusBatchRequest.Details.Count() > 0)
                {
                    foreach (var item in changeOrderStatusBatchRequest.Details)
                    {
                        var detail = new OrderBatchDetailRequest();
                        detail.TrackingNumber1 = item.TrackingNumber1;
                        detail.OrderKey = item.OrderKey;
                        details.Add(detail);
                    }
                }
                req.Details = details.ToArray();
                res = res = await _exigoApiContext.GetContext(false).ChangeOrderStatusBatchAsync(req);

            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        ///CreatePayment
        /// </summary>
        /// <param name="CreatePaymentRequest"></param>
        /// <returns>CreatePaymentResponse</returns>
        public async Task<Exigo.Api.Client.CreatePaymentResponse> CreatePayment(Exigo.Api.Client.CreatePaymentRequest createPaymentRequest)
        {
            var res = new Exigo.Api.Client.CreatePaymentResponse();
            try
            {
                //Create Request
                var req = new Exigo.Api.Client.CreatePaymentRequest();
                //req.PaymentDate = DateTime.Today;
                //req.Amount = 1;
                //req.PaymentType = PaymentType.Cash;
                //req.OrderKey = "1";             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.

                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentRequest.Amount;
                req.PaymentType = PaymentType.Cash;
                req.OrderKey = createPaymentRequest.OrderKey;
                //Send Request to Server and Get Response
                res = await _exigoApiContext.GetContext(false).CreatePaymentAsync(req);

            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;

        }

        /// <summary>
        ///CreatePaymentCreditCard
        /// </summary>
        /// <param name="CreatePaymentCreditCardRequest"></param>
        /// <returns>CreatePaymentCreditCardResponse</returns>
        public async Task<CreatePaymentCreditCardResponse> CreatePaymentCreditCard(CreatePaymentCreditCardRequest createPaymentCreditCardRequest)
        {
            var res = new CreatePaymentCreditCardResponse();
            try
            {
                //Create Request

                var req = new CreatePaymentCreditCardRequest();
                //req.OrderID = 1;                //Unique numeric identifier for order record.
                //req.PaymentDate = DateTime.Today;
                //req.Amount = 1;
                //req.CreditCardNumber = "1";
                //req.ExpirationMonth = 1;
                //req.ExpirationYear = 1;
                //req.BillingName = "1";
                //req.BillingAddress = "1";
                //req.BillingAddress2 = "1";
                //req.BillingCity = "1";
                //req.BillingZip = "1";
                //req.AuthorizationCode = "1";
                //req.Memo = "1";
                //req.OrderKey = "1";             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                //req.MerchantTransactionKey = "1";

                req.OrderID = createPaymentCreditCardRequest.OrderID;                //Unique numeric identifier for order record.
                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentCreditCardRequest.Amount;
                req.CreditCardNumber = createPaymentCreditCardRequest.CreditCardNumber;
                req.ExpirationMonth = createPaymentCreditCardRequest.ExpirationMonth;
                req.ExpirationYear = createPaymentCreditCardRequest.ExpirationYear;
                req.BillingName = createPaymentCreditCardRequest.BillingName;
                req.BillingAddress = createPaymentCreditCardRequest.BillingAddress;
                req.BillingAddress2 = createPaymentCreditCardRequest.BillingAddress2;
                req.BillingCity = createPaymentCreditCardRequest.BillingCity;
                req.BillingZip = createPaymentCreditCardRequest.BillingZip;
                req.AuthorizationCode = createPaymentCreditCardRequest.AuthorizationCode;
                req.Memo = createPaymentCreditCardRequest.Memo;
                req.OrderKey = createPaymentCreditCardRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                req.MerchantTransactionKey = createPaymentCreditCardRequest.MerchantTransactionKey;



                //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).CreatePaymentCreditCardAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
                throw;
            }
            return res;

        }

        /// <summary>
        ///CreatePaymentWallet
        /// </summary>
        /// <param name="CreatePaymentWalletRequest"></param>
        /// <returns>CreatePaymentResponse</returns>
        public async Task<Exigo.Api.Client.CreatePaymentResponse> CreatePaymentWallet(CreatePaymentWalletRequest createPaymentWalletRequest)
        {
            var res = new Exigo.Api.Client.CreatePaymentResponse();
            try
            {
                var req = new CreatePaymentWalletRequest();
                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentWalletRequest.Amount;
                req.WalletAccount = createPaymentWalletRequest.WalletAccount;
                req.AuthorizationCode = createPaymentWalletRequest.AuthorizationCode;
                req.Memo = createPaymentWalletRequest.Memo;
                req.BillingName = createPaymentWalletRequest.BillingName;
                req.WalletType = createPaymentWalletRequest.WalletType;
                req.OrderKey = createPaymentWalletRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.

                //Send Request to Server and Get Response
                res = await _exigoApiContext.GetContext(false).CreatePaymentWalletAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }

            return res;
        }

        /// <summary>
        ///ValidateCreditCardToken
        /// </summary>
        /// <param name="ValidateCreditCardTokenRequest"></param>
        /// <returns>ValidateCreditCardTokenResponse</returns>
        public async Task<ValidateCreditCardTokenResponse> ValidateCreditCardToken(ValidateCreditCardTokenRequest creditCardTokenRequest)
        {
            var res = new ValidateCreditCardTokenResponse();
            try
            {
                var req = new ValidateCreditCardTokenRequest();
                req.CreditCardIdentifier = creditCardTokenRequest.CreditCardIdentifier;
                req.ExpirationYear = creditCardTokenRequest.ExpirationYear;
                req.ExpirationMonth = creditCardTokenRequest.ExpirationMonth;
                req.CvcCode = creditCardTokenRequest.CvcCode;
                req.BillingName = creditCardTokenRequest.BillingName;
                req.BillingAddress1 = creditCardTokenRequest.BillingAddress1;
                req.BillingAddress2 = creditCardTokenRequest.BillingAddress2;
                req.BillingCity = creditCardTokenRequest.BillingCity;
                req.BillingZip = creditCardTokenRequest.BillingZip;
                req.CustomerID = creditCardTokenRequest.CustomerID;             //Unique numeric identifier for a customer record.
                //req.CustomerKey = creditCardTokenRequest.CustomerKey;//Unique alpha numeric identifier for customer record. Exeption will occur if CustomerID & CustomerKey are provided.
                //req.OrderID = creditCardTokenRequest.OrderID;                //Unique numeric identifier for order record.
                req.OrderKey = creditCardTokenRequest.OrderKey; //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                req.ValidateTokenOnFile = creditCardTokenRequest.ValidateTokenOnFile;    //Optional. Use this option to validate information already on file
                req.AccountOnFile = creditCardTokenRequest.AccountOnFile;          //Optional. To be used when ValidateTokenOnFile = true
                req.WarehouseID = creditCardTokenRequest.WarehouseID;            //Optional
                req.CurrencyCode = creditCardTokenRequest.CurrencyCode;         //Optional
                req.Email = creditCardTokenRequest.Email;                //Optional
                req.Phone = creditCardTokenRequest.Phone;                //Optional
                req.ClientIpAddress = creditCardTokenRequest.ClientIpAddress;      //Optional

                //Send Request to Server and Get Response
                res = await _exigoApiContext.GetContext(false).ValidateCreditCardTokenAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
                throw;
            }


            return res;
        }

        /// <summary>
        ///CreatePaymentPointAccount
        /// </summary>
        /// <param name="CreatePaymentPointAccountRequest"></param>
        /// <returns>CreatePaymentPointAccountResponse</returns>
        public async Task<CreatePaymentPointAccountResponse> CreatePaymentPointAccount(CreatePaymentPointAccountRequest createPaymentPointAccountRequest)
        {
            var res = new CreatePaymentPointAccountResponse();
            try
            {
                //Create Request
                var req = new CreatePaymentPointAccountRequest();
                req.OrderID = createPaymentPointAccountRequest.OrderID;
                req.PointAccountID = createPaymentPointAccountRequest.PointAccountID;
                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentPointAccountRequest.Amount;
                req.OrderKey = createPaymentPointAccountRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                                                                                      //Send Request to Server and Get Response
                res = await _exigoApiContext.GetContext(false).CreatePaymentPointAccountAsync(req);
                //Now examine the results:
                //Console.WriteLine("PaymentID: {0}", res.PaymentID);
                //Console.WriteLine("Message: {0}", res.Message);
                //Console.WriteLine("DisplayMessage: {0}", res.DisplayMessage);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return res;
        }

        /// <summary>
        ///CreatePaymentCheck
        /// </summary>
        /// <param name="CreatePaymentCheckRequest"></param>
        /// <returns>CreatePaymentCheckResponse</returns>
        public async Task<CreatePaymentCheckResponse> CreatePaymentCheck(CreatePaymentCheckRequest createPaymentCheckRequest)
        {
            var res = new CreatePaymentCheckResponse();
            try
            {
                var req = new CreatePaymentCheckRequest();
                req.OrderID = createPaymentCheckRequest.OrderID;
                req.PaymentDate = DateTime.Today;
                req.Amount = createPaymentCheckRequest.Amount;
                req.OrderKey = createPaymentCheckRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                                                                               //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).CreatePaymentCheckAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        ///ChargeCreditCardToken
        /// </summary>
        /// <param name="ChargeCreditCardTokenRequest"></param>
        /// <returns>ChargeCreditCardResponse</returns>
        public async Task<ChargeCreditCardResponse> ChargeCreditCardToken(ChargeCreditCardTokenRequest chargeCreditCardTokenRequest)
        {
            var res = new ChargeCreditCardResponse();
            try
            {
                var req = new ChargeCreditCardTokenRequest();
                req.CreditCardToken = chargeCreditCardTokenRequest.CreditCardToken;
                req.BillingName = chargeCreditCardTokenRequest.BillingName;
                req.BillingAddress = chargeCreditCardTokenRequest.BillingAddress;
                req.BillingAddress2 = chargeCreditCardTokenRequest.BillingAddress2;
                req.BillingCity = chargeCreditCardTokenRequest.BillingCity;
                req.BillingZip = chargeCreditCardTokenRequest.BillingZip;
                req.ExpirationMonth = chargeCreditCardTokenRequest.ExpirationMonth;
                req.ExpirationYear = chargeCreditCardTokenRequest.ExpirationYear;
                req.OrderKey = chargeCreditCardTokenRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                                                                                  //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).ChargeCreditCardTokenAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
                //throw;
            }
            return res;


        }

        /// <summary>
        ///ChargeCreditCardTokenOnFile
        /// </summary>
        /// <param name="ChargeCreditCardTokenOnFileRequest"></param>
        /// <returns>ChargeCreditCardResponse</returns>
        public async Task<ChargeCreditCardResponse> ChargeCreditCardTokenOnFile(ChargeCreditCardTokenOnFileRequest chargeCreditCardTokenOnFileRequest)
        {
            var res = new ChargeCreditCardResponse();

            try
            {
                //Create Request
                var req = new ChargeCreditCardTokenOnFileRequest();
                req.CreditCardAccountType = AccountCreditCardType.Primary;

                req.OrderKey = chargeCreditCardTokenOnFileRequest.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.

                //Send Request to Server and Get Response
                res = await _exigoApiContext.GetContext(false).ChargeCreditCardTokenOnFileAsync(req);


            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;


        }

        /// <summary>
        ///ChargeGroupOrderCreditCardToken
        /// </summary>
        /// <param name="ChargeGroupOrderCreditCardTokenRequest"></param>
        /// <returns>ChargeGroupOrderCreditCardTokenResponse</returns>
        public async Task<ChargeGroupOrderCreditCardTokenResponse> ChargeGroupOrderCreditCardToken(ChargeGroupOrderCreditCardTokenRequest chargeGroupOrderCredit)
        {
            var chargeGroupOrderCreditCardTokenResponse = new ChargeGroupOrderCreditCardTokenResponse();
            try
            {
                var req = new ChargeGroupOrderCreditCardTokenRequest();
                req.CreditCardToken = chargeGroupOrderCredit.CreditCardToken;
                req.BillingName = chargeGroupOrderCredit.BillingName;
                req.BillingAddress = chargeGroupOrderCredit.BillingAddress;
                req.BillingAddress2 = chargeGroupOrderCredit.BillingAddress2;
                req.BillingCity = chargeGroupOrderCredit.BillingCity;
                req.BillingZip = chargeGroupOrderCredit.BillingZip;
                req.CvcCode = chargeGroupOrderCredit.CvcCode;
                req.IssueNumber = chargeGroupOrderCredit.IssueNumber;
                //Add Orders
                var orders = new List<GroupOrder>();

                if (chargeGroupOrderCredit.Orders.Count() > 0)
                {
                    foreach (var item in chargeGroupOrderCredit.Orders)
                    {
                        var order = new GroupOrder();
                        order.MaxAmount = item.MaxAmount;
                        order.OrderKey = item.OrderKey;
                        orders.Add(order);
                    }
                }
                //var order1 = new GroupOrder();
                //order1.MaxAmount = 1;
                //order1.OrderKey = "1";
                //orders.Add(order1);
                //var order2 = new GroupOrder();
                //order2.MaxAmount = 2;
                //order2.OrderKey = "2";
                //orders.Add(order2);
                //Now attach the list to the request
                req.Orders = orders.ToArray();
                req.ClientIPAddress = chargeGroupOrderCredit.ClientIPAddress;
                req.MasterOrderKey = chargeGroupOrderCredit.MasterOrderKey;       //Unique alpha numeric identifier for master order record. Exeption will occur if MasterOrderID & MasterOrderKey are provided.
                                                                                  //Send Request to Server and Get Response

                chargeGroupOrderCreditCardTokenResponse = await _exigoApiContext.GetContext(false).ChargeGroupOrderCreditCardTokenAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return chargeGroupOrderCreditCardTokenResponse;
        }

        /// <summary>
        ///RefundPriorCreditCardCharge
        /// </summary>
        /// <param name="RefundPriorCreditCardChargeRequest"></param>
        /// <returns>RefundPriorCreditCardChargeResponse</returns>
        public async Task<RefundPriorCreditCardChargeResponse> RefundPriorCreditCardCharge(RefundPriorCreditCardChargeRequest refundPriorCredit)
        {
            var refundPriorCreditCardChargeResponse = new RefundPriorCreditCardChargeResponse();
            try
            {
                //Create Request
                var req = new RefundPriorCreditCardChargeRequest();
                req.ReturnPaymentID = refundPriorCredit.ReturnPaymentID;
                req.OrderID = refundPriorCredit.OrderID;                //Unique numeric identifier for order record.
                req.MaxAmount = refundPriorCredit.MaxAmount;
                req.OrderKey = refundPriorCredit.OrderKey;             //Unique alpha numeric identifier for order record. Exeption will occur if OrderID & OrderKey are provided.
                                                                       //Send Request to Server and Get Response

                refundPriorCreditCardChargeResponse = await _exigoApiContext.GetContext(false).RefundPriorCreditCardChargeAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return refundPriorCreditCardChargeResponse;
        }

        public async Task<VerifyAddressResponse> Shipping(VerifyAddressRequest addressRequest)
        {
            var verifyAddressResponse = new VerifyAddressResponse();
            try
            {
                var req = new VerifyAddressRequest();

                req.Address = addressRequest.Address;
                req.State = addressRequest.State;
                req.City = addressRequest.City;
                req.Zip = addressRequest.Zip;
                req.Country = addressRequest.Country;
                //Send Request to Server and Get Response

                verifyAddressResponse = await _exigoApiContext.GetContext(false).VerifyAddressAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return verifyAddressResponse;
        }

        

        public List<Address> GetCustomerAddress(int customerID)
        {
            // Address address = new Address();
            // address = DAL.GetCustomerAddresses(Identity.Customer.CustomerID)
            //.Where(c => c.IsComplete)
            //.Select(c => c as ShippingAddress);
            using (var context = Common.Utils.DbConnection.Sql())
            {
                var addresses = new List<Address>();
                try
                {
                    var model = context.Query(@"
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
                            ", new { customerID }).FirstOrDefault();

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
                catch (Exception ex)
                {
                    ex.Message.ToString();
                }
                return addresses;

                //return addresses;
                //var response = context.Query<Address>(QueryUtility.getUserAddress_Query, new
                //{
                //    warehouse = 1,
                //    currencyCode = "usd",
                //    languageID = 0,
                //    priceTypeID = 1,
                //    itemCodes = itemCodes
                //}).ToList();
                //return response[0];
            }
        }

        public async Task<GetWarehousesResponse> GetWarehouses(GetWarehousesRequest warehousesRequest)
        {
            //Create Request

            var req = new GetWarehousesRequest();
            req.WarehouseId = warehousesRequest.WarehouseId;
            //Send Request to Server and Get Response

            var res = await _exigoApiContext.GetContext(false).GetWarehousesAsync(req);
            return res;
        }

        public async Task<GetOrdersResponse> GetOrder(GetOrdersRequest ordersRequest, int customerId)
        {
            var req = new GetOrdersRequest();
            req.CustomerID = customerId;             //Unique numeric identifier for customer record.
            req.OrderID = 266902;                //Unique numeric identifier for order record.
            req.OrderDateStart = DateTime.Today;         //If supplied only orders greater than or equal to will be returned.
            req.OrderDateEnd = DateTime.Today;
            req.WarehouseID = 1;            //Unique location for orders
            req.CurrencyCode = "usd";
            //Send Request to Server and Get Response

            var res = await _exigoApiContext.GetContext(false).GetOrdersAsync(req);

            return res;
        }

        /// <summary>
        /// GetSpecialItem
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>

        public ShopProductsResponse GetSpecialItem()
        {
            //dynamic response;
            using (var context = Common.Utils.DbConnection.Sql())
            {
                var response = context.Query<ShopProductsResponse>(QueryUtility.GetSpecialItem_Query, new
                {
                    warehouse = _orderConfiguration.WarehouseID,
                    currencyCode = _orderConfiguration.CurrencyCode,
                    languageID = _orderConfiguration.LanguageID,
                    priceTypeID = _orderConfiguration.PriceTypeID
                }).ToList();
                return response[0];
            }
        }

        #region Private methods

        public static List<ItemCategoryResponse> GetWebCategoriesRecursively(int webCategoryID)
        {
            var categories = new List<ItemCategoryResponse>();

            using (var context = Common.Utils.DbConnection.Sql())
            {
                var data = context.Query<ItemCategoryResponse>(@"
			        ;WITH webcat (WebCategoryID, WebCategoryDescription, ParentID, NestedLevel, SortOrder) 
				         AS (SELECT WebCategoryID, 
							        WebCategoryDescription, 
							        ParentID = COALESCE(ParentID, 0), 
							        NestedLevel,
                                    SortOrder
					         FROM   WebCategories 
					         WHERE  WebCategoryID = @webcategoryid
							        AND WebID = @webid 
					         UNION ALL 
					         SELECT w.WebCategoryID, 
							        w.WebCategoryDescription, 
							        w.ParentID, 
							        w.NestedLevel,
                                    w.SortOrder
					         FROM   WebCategories w 
							        INNER JOIN webcat c 
									        ON c.WebCategoryID = w.ParentID) 
			        SELECT * 
			        FROM   webcat 
		        ", new
                {
                    webid = 1,
                    webcategoryid = webCategoryID
                }).ToList();

                categories = GetWebCategoriesRecursively(categories, data, webCategoryID);
            }

            return categories.OrderBy(c => c.SortOrder).ToList();
        }

        // Non Action Method to add the New Address of Customer
        [System.Web.Http.NonAction]
        public async Task<Address> SaveNewCustomerAddress(int customerID, Address address)
        {
            var addressesOnFile = GetCustomerAddress(customerID).Where(c => c.IsComplete);

            try
            {
                // Do any of the addresses on file match the one we are using?
                // If not, save this address to the next available slot
                if (!addressesOnFile.Any(c => c.Equals(address)))
                {
                    var saveAddress = false;
                    var request = new UpdateCustomerRequest();
                    request.CustomerID = customerID;

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
            catch (Exception e)
            {

                e.Message.ToString();
                throw;
            }


        }

        //public static List<Address> GetCustomerAddress1(int customerID)
        //{
        //    // Address address = new Address();
        //    // address = DAL.GetCustomerAddresses(Identity.Customer.CustomerID)
        //    //.Where(c => c.IsComplete)
        //    //.Select(c => c as ShippingAddress);
        //    using (var context = Common.Utils.DbConnection.Sql())
        //    {
        //        var addresses = new List<Address>();
        //        try
        //        {
        //            var model = context.Query(@"
        //                    select 
        //                        c.FirstName,
        //                        c.LastName,
        //                        c.Email,
        //                        c.Phone,

        //                        c.MainAddress1,
        //                        c.MainAddress2,
        //                        c.MainCity,
        //                        c.MainState,
        //                        c.MainZip,
        //                        c.MainCountry,

        //                        c.MailAddress1,
        //                        c.MailAddress2,
        //                        c.MailCity,
        //                        c.MailState,
        //                        c.MailZip,
        //                        c.MailCountry,

        //                        c.OtherAddress1,
        //                        c.OtherAddress2,
        //                        c.OtherCity,
        //                        c.OtherState,
        //                        c.OtherZip,
        //                        c.OtherCountry

        //                    from Customers c
        //                    where c.CustomerID = @customerID
        //                    ", new { customerID }).FirstOrDefault();

        //            addresses.Add(new ShippingAddress()
        //            {
        //                AddressType = AddressType.Main,
        //                FirstName = model.FirstName,
        //                LastName = model.LastName,
        //                Email = model.Email,
        //                Phone = model.Phone,
        //                Address1 = model.MainAddress1,
        //                Address2 = model.MainAddress2,
        //                City = model.MainCity,
        //                State = model.MainState,
        //                Zip = model.MainZip,
        //                Country = model.MainCountry
        //            });

        //            addresses.Add(new ShippingAddress()
        //            {
        //                AddressType = AddressType.Mailing,
        //                FirstName = model.FirstName,
        //                LastName = model.LastName,
        //                Email = model.Email,
        //                Phone = model.Phone,
        //                Address1 = model.MailAddress1,
        //                Address2 = model.MailAddress2,
        //                City = model.MailCity,
        //                State = model.MailState,
        //                Zip = model.MailZip,
        //                Country = model.MailCountry
        //            });

        //            addresses.Add(new ShippingAddress()
        //            {
        //                AddressType = AddressType.Other,
        //                FirstName = model.FirstName,
        //                LastName = model.LastName,
        //                Email = model.Email,
        //                Phone = model.Phone,
        //                Address1 = model.OtherAddress1,
        //                Address2 = model.OtherAddress2,
        //                City = model.OtherCity,
        //                State = model.OtherState,
        //                Zip = model.OtherZip,
        //                Country = model.OtherCountry
        //            });

        //        }
        //        catch
        //        {

        //        }
        //        return addresses;

        //        //return addresses;
        //        //var response = context.Query<Address>(QueryUtility.getUserAddress_Query, new
        //        //{
        //        //    warehouse = 1,
        //        //    currencyCode = "usd",
        //        //    languageID = 0,
        //        //    priceTypeID = 1,
        //        //    itemCodes = itemCodes
        //        //}).ToList();
        //        //return response[0];
        //    }
        //}

        [System.Web.Http.NonAction]
        public IEnumerable<ShopProductsResponse> GetItems(GetItemListRequest request, bool includeItemDescriptions = true)
        {
            var tempCategoryIDs = new List<int>();
            var categoryIDs = new List<int>();
            if (request.CategoryID != null)
            {
                // Get all category ids underneath the request's category id
                if (request.IncludeChildCategories)
                {
                    using (var context = Common.Utils.DbConnection.Sql())
                    {


                        categoryIDs.AddRange(context.Query<int>(QueryUtility.categoryIdList_Query, new
                        {
                            webid = 1,
                            masterCategoryID = request.CategoryID
                        }).ToList());
                    }
                }
                else
                {
                    categoryIDs.Add(Convert.ToInt32(request.CategoryID));
                }
            }

            // If we requested specific categories, get the item codes in the categories
            if (categoryIDs.Count > 0)
            {
                var categoryItemCodes = new List<string>();

                using (var context = Common.Utils.DbConnection.Sql())
                {
                    categoryItemCodes = context.Query<string>(QueryUtility.categoryItemCodesList_Query, new
                    {
                        webid = 1,
                        webcategoryids = categoryIDs
                    }).ToList();
                }

                var existingItemCodes = request.ItemCodes.ToList();
                existingItemCodes.AddRange(categoryItemCodes);
                request.ItemCodes = existingItemCodes.ToArray();
            }

            // Do a final check to ensure if the category we are looking at does not contain a item directly nested within it, we pull back the first child category
            if (request.ItemCodes.Length == 0 && request.CategoryID != null)
            {
                var tempItemCodeList = new List<string>();
                using (var context = Common.Utils.DbConnection.Sql())
                {
                    tempItemCodeList = context.Query<string>(QueryUtility.tempItemCodeList_Query, new
                    {
                        webid = 1,
                        masterCategoryID = request.CategoryID
                    }).ToList();
                }

                request.ItemCodes = tempItemCodeList.ToArray();
            }


            // If we don't have any items, stop here.
            if (request.ItemCodes.Length == 0) yield break;

            // get the item information             
            var priceTypeID = request.PriceTypeID;

            var items = GetItemInformation(request, priceTypeID);  //: GetItemList(request, priceTypeID);

            // Populate the group members and dynamic kits
            if (items.Any())
            {
                // PopulateAdditionalItemData(items, request);
            }

            if (request.SortBy == 1)
            {
                // Newest Arrivals
                items = items.OrderByDescending(x => x.ItemID).ToList();
            }
            if (request.SortBy == 2)
            {
                // Price: $ - $$
                items = items.OrderBy(x => x.Price).ToList();
            }
            else if (request.SortBy == 3)
            {
                // Price: $$ - $
                items = items.OrderByDescending(x => x.Price).ToList();
            }
            else if (request.SortBy == 4)
            {
                // Name: A - Z
                items = items.OrderBy(x => x.ItemDescription).ToList();
            }
            else
            {
                // Featured          
            }

            // Return the data
            foreach (var item in items)
            {
                yield return item;
            }
        }

        [System.Web.Http.NonAction]
        private static List<ShopProductsResponse> GetItemInformation(GetItemListRequest request, int priceTypeID)
        {
            try
            {
                var apiItems = new List<ShopProductsResponse>();
                string sorting = string.Empty;
                sorting = request.SortBy switch
                {
                    2 => "ip.price, i.itemcode desc",// Price: $ - $$
                    3 => "ip.price desc, i.itemcode desc",// Price: $$ - $
                    4 => "i.ItemDescription, i.itemcode",// Name: A - Z
                    _ => "i.itemId desc",// Newest Arrivals
                };
                int languageID = request.LanguageID;
                List<string> itemCodes = request.ItemCodes.ToList();

                using var context = Common.Utils.DbConnection.Sql();
                apiItems = context.Query<ShopProductsResponse>(QueryUtility.apiItemsListForProducts_Query + sorting + @"
                            ", new
                {
                    warehouse = 1,
                    currencyCode = "usd",
                    languageID = languageID,
                    priceTypeID = 1,
                    itemCodes = itemCodes
                }).ToList();

                var length = request.ItemCodes.Count();
                var orderedItems = new List<ShopProductsResponse>();
                // Handle Sorting here, the sort order was based on the order of the Item Codes passed in originally
                for (var i = 0; i < length; i++)
                {
                    var matchingItem = apiItems.FirstOrDefault(c => c.ItemCode == request.ItemCodes[i]);

                    if (matchingItem != null)
                    {
                        orderedItems.Add(matchingItem);
                    }
                }

                foreach (var item in orderedItems)
                {
                    //item.ProductImage = ProductImageUtility.GetProductImageUtility(item.LargeImageUrl);
                    item.ProductImage = null;
                }


                var data = orderedItems;
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [System.Web.Http.NonAction]
        private static List<ItemCategoryResponse> GetWebCategoriesRecursively(List<ItemCategoryResponse> categories, List<ItemCategoryResponse> data, int parentID)
        {
            foreach (var category in data.Where(c => c.ParentID == parentID))
            {
                categories.Add(category);
                if (data.Count(c => c.ParentID == category.WebCategoryID) > 0)
                {
                    GetWebCategoriesRecursively(category.Subcategories, data, category.WebCategoryID);
                }
            }

            return categories.OrderBy(c => c.SortOrder).ToList();
        }

        #endregion

        //To Get customer detail for editing.
        public async Task<GetCustomersResponse> GetCustomerRealTime(int customerID)
        {
            var req = new GetCustomersRequest();
            req.CustomerID = customerID;
            var response = await _exigoApiContext.GetContext(false).GetCustomersAsync(req);
            return response;
        }

        /// <summary>
        /// UpdateCustomer
        /// </summary>
        /// <param name="updateCustomerRequest"></param>
        /// <returns></returns>
        public async Task<UpdateCustomerResponse> UpdateCustomer(UpdateCustomerRequest updateCustomerRequest)
        {
            try
            {
                var request = new UpdateCustomerRequest();
                request.CustomerID = updateCustomerRequest.CustomerID;
                request.FirstName = updateCustomerRequest.FirstName;
                request.LastName = updateCustomerRequest.LastName;
                request.LoginName = updateCustomerRequest.LoginName;
                request.LoginPassword = updateCustomerRequest.LoginPassword;
                request.MailAddress1 = updateCustomerRequest.MailAddress1;
                request.MailAddress2 = updateCustomerRequest.MailAddress2;
                request.MailAddress3 = updateCustomerRequest.MailAddress3;
                request.MainAddress1 = updateCustomerRequest.MainAddress1;
                request.MainAddress2 = updateCustomerRequest.MainAddress2;
                request.MainAddress3 = updateCustomerRequest.MainAddress3;
                request.OtherAddress1 = updateCustomerRequest.OtherAddress1;
                request.OtherAddress2 = updateCustomerRequest.OtherAddress2;
                request.OtherAddress3 = updateCustomerRequest.OtherAddress3;
                request.Email = updateCustomerRequest.Email;
                request.Phone = updateCustomerRequest.Phone;
                request.MobilePhone = updateCustomerRequest.MobilePhone;

                var response = await _exigoApiContext.GetContext(false).UpdateCustomerAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// PromoCode
        /// </summary>
        /// <param name="promoCode"></param>
        /// <returns></returns>
        public PromoCode GetPromoDetail(string promoCode, int customerId)
        {
            //dynamic response;
            using (var context = Common.Utils.DbConnection.Sql())
            {
                 // var cCodes = context.Query<PromoCode>(QueryUtility.GetCoupenCodes).ToList();
                var coupon = context.Query<PromoCode>(QueryUtility.GetCoupenCode_Query, new
                {
                    Code = promoCode
                }).FirstOrDefault();

                // Get Customer type from MemoryCache
                int customerType = _exigoApiContext.GetContext(false).GetCustomersAsync(new GetCustomersRequest { CustomerID = customerId }).Result.Customers[0].CustomerType;
                if (coupon != null)
                {
                    if (!string.IsNullOrEmpty(coupon.CustomerTypes))
                    {
                        var customerTypes = coupon.CustomerTypes.Split(',').ToArray();

                        if (!customerTypes.Contains(customerType.ToString()))
                        {
                            return new PromoCode { ErrorMessage = "Oops. Coupon code " + promoCode + " is not valid for this user." };
                        }
                    }
                }
                else
                {
                    return new PromoCode { ErrorMessage = "Oops. Coupon code " + promoCode + " is not valid." };
                }
                return coupon;
            }
        }

        /// <summary>
        /// Create Item
        /// </summary>
        /// <param name="createItemRequest"></param>
        /// <returns></returns>
        public async Task<CreateItemResponse> CreateItem(CreateItemRequest createItemRequest)
        {
            var res = new CreateItemResponse();
            try
            {
                // Create request
                var req = new CreateItemRequest();

                req.ItemCode = createItemRequest.ItemCode;
                req.Description = createItemRequest.Description;
                req.Weight = createItemRequest.Weight;
                req.Notes = createItemRequest.Notes;
                req.AvailableInAllCountryRegions = createItemRequest.AvailableInAllCountryRegions;
                req.TaxedInAllCountryRegions = createItemRequest.TaxedInAllCountryRegions;
                req.AvailableInAllWarehouses = createItemRequest.AvailableInAllWarehouses;
                req.IsVirtual = createItemRequest.IsVirtual;
                req.ItemTypeID = createItemRequest.ItemTypeID;
                req.IsSubscriptionUpdate = createItemRequest.IsSubscriptionUpdate;
                req.IsPointIncrement = createItemRequest.IsPointIncrement;
                req.OtherCheck1 = createItemRequest.OtherCheck1;
                req.OtherCheck2 = createItemRequest.OtherCheck2;
                req.OtherCheck3 = createItemRequest.OtherCheck3;
                req.OtherCheck4 = createItemRequest.OtherCheck4;
                req.OtherCheck5 = createItemRequest.OtherCheck5;
                req.Field1 = createItemRequest.Field1;
                req.Field2 = createItemRequest.Field2;
                req.Field3 = createItemRequest.Field3;
                req.Field4 = createItemRequest.Field4;
                req.Field5 = createItemRequest.Field5;
                req.Field6 = createItemRequest.Field6;
                req.Field7 = createItemRequest.Field7;
                req.Field8 = createItemRequest.Field8;
                req.Field9 = createItemRequest.Field9;
                req.Field10 = createItemRequest.Field10;
                req.HideFromSearch = createItemRequest.HideFromSearch;

                //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).CreateItemAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// Update Item
        /// </summary>
        /// <param name="updateItemRequest"></param>
        /// <returns></returns>
        public async Task<UpdateItemResponse> UpdateItem(UpdateItemRequest updateItemRequest)
        {
            var res = new UpdateItemResponse();
            try

            {
                //Create Request
                var req = new UpdateItemRequest();

                req.ItemCode = updateItemRequest.ItemCode;
                req.Description = updateItemRequest.Description;
                req.Weight = updateItemRequest.Weight;
                req.Notes = updateItemRequest.Notes;
                req.AvailableInAllCountryRegions = updateItemRequest.AvailableInAllCountryRegions;
                req.TaxedInAllCountryRegions = updateItemRequest.TaxedInAllCountryRegions;
                req.AvailableInAllWarehouses = updateItemRequest.AvailableInAllWarehouses;
                req.IsVirtual = updateItemRequest.IsVirtual;
                req.ItemTypeID = updateItemRequest.ItemTypeID;
                req.ShortDetail = updateItemRequest.ShortDetail;
                req.ShortDetail2 = updateItemRequest.ShortDetail2;
                req.ShortDetail3 = updateItemRequest.ShortDetail3;
                req.ShortDetail4 = updateItemRequest.ShortDetail4;
                req.LongDetail = updateItemRequest.LongDetail;
                req.LongDetail2 = updateItemRequest.LongDetail2;
                req.LongDetail3 = updateItemRequest.LongDetail3;
                req.LongDetail4 = updateItemRequest.LongDetail4;
                req.OtherCheck1 = updateItemRequest.OtherCheck1;
                req.OtherCheck2 = updateItemRequest.OtherCheck2;
                req.OtherCheck3 = updateItemRequest.OtherCheck3;
                req.OtherCheck4 = updateItemRequest.OtherCheck4;
                req.OtherCheck5 = updateItemRequest.OtherCheck5;
                req.Field1 = updateItemRequest.Field1;
                req.Field2 = updateItemRequest.Field2;
                req.Field3 = updateItemRequest.Field3;
                req.Field4 = updateItemRequest.Field4;
                req.Field5 = updateItemRequest.Field5;
                req.Field6 = updateItemRequest.Field6;
                req.Field7 = updateItemRequest.Field7;
                req.Field8 = updateItemRequest.Field8;
                req.Field9 = updateItemRequest.Field9;
                req.Field10 = updateItemRequest.Field10;
                req.HideFromSearch = updateItemRequest.HideFromSearch;
                req.IsSubscriptionUpdate = updateItemRequest.IsSubscriptionUpdate;
                req.IsPointIncrement = updateItemRequest.IsPointIncrement;

                //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).UpdateItemAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// Set Item Price
        /// </summary>
        /// <param name="setItemPriceRequest"></param>
        /// <returns></returns>
        public async Task<SetItemPriceResponse> SetItemPrice(SetItemPriceRequest setItemPriceRequest)
        {
            var res = new SetItemPriceResponse();
            try
            {
                // Create request
                var req = new SetItemPriceRequest();
                req.ItemCode = setItemPriceRequest.ItemCode;
                req.CurrencyCode = setItemPriceRequest.CurrencyCode;
                req.PriceType = setItemPriceRequest.PriceType;              //Controls which price band to use
                req.Price = setItemPriceRequest.Price;                  //Price amount
                req.BusinessVolume = setItemPriceRequest.BusinessVolume;         //Price amount
                req.CommissionableVolume = setItemPriceRequest.CommissionableVolume;   //Price amount
                req.TaxablePrice = setItemPriceRequest.TaxablePrice;           //Price amount
                req.ShippingPrice = setItemPriceRequest.ShippingPrice;          //Price amount
                req.Other1Price = setItemPriceRequest.Other1Price;            //Price amount
                req.Other2Price = setItemPriceRequest.Other2Price;            //Price amount
                req.Other3Price = setItemPriceRequest.Other3Price;            //Price amount
                req.Other4Price = setItemPriceRequest.Other4Price;            //Price amount
                req.Other5Price = setItemPriceRequest.Other5Price;            //Price amount
                req.Other6Price = setItemPriceRequest.Other6Price;            //Price amount
                req.Other7Price = setItemPriceRequest.Other7Price;            //Price amount
                req.Other8Price = setItemPriceRequest.Other8Price;            //Price amount
                req.Other9Price = setItemPriceRequest.Other9Price;            //Price amount
                req.Other10Price = setItemPriceRequest.Other10Price;           //Price amount

                //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).SetItemPriceAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// Set Item Warehouse
        /// </summary>
        /// <param name="setItemWarehouseRequest"></param>
        /// <returns></returns>
        public async Task<SetItemWarehouseResponse> SetItemWarehouse(SetItemWarehouseRequest setItemWarehouseRequest)
        {
            var res = new SetItemWarehouseResponse();
            try
            {
                //Create Request
                var req = new SetItemWarehouseRequest();
                req.AllowedUserWarehouses = setItemWarehouseRequest.AllowedUserWarehouses;
                req.AllowedWarehouseManagementTypes = setItemWarehouseRequest.AllowedWarehouseManagementTypes;
                req.ItemCode = setItemWarehouseRequest.ItemCode;
                req.WarehouseID = setItemWarehouseRequest.WarehouseID;            //Unique location for orders
                req.IsAvailable = setItemWarehouseRequest.IsAvailable;
                req.ItemManageTypeID = setItemWarehouseRequest.ItemManageTypeID;       //Item management type

                //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).SetItemWarehouseAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// Set Item Country Region
        /// </summary>
        /// <param name="setItemCountryRegionRequest"></param>
        /// <returns></returns>
        public async Task<SetItemCountryRegionResponse> SetItemCountryRegion(SetItemCountryRegionRequest setItemCountryRegionRequest)
        {
            var res = new SetItemCountryRegionResponse();
            try
            {
                //Create Request
                var req = new SetItemCountryRegionRequest();
                req.ItemCode = setItemCountryRegionRequest.ItemCode;
                req.CountryCode = setItemCountryRegionRequest.CountryCode;
                req.RegionCode = setItemCountryRegionRequest.RegionCode;
                req.Taxed = setItemCountryRegionRequest.Taxed;
                req.TaxedFed = setItemCountryRegionRequest.TaxedFed;
                req.TaxedState = setItemCountryRegionRequest.TaxedState;
                req.UseTaxOverride = setItemCountryRegionRequest.UseTaxOverride;
                req.TaxOverridePct = setItemCountryRegionRequest.TaxOverridePct;
                req.BulkUpdate = setItemCountryRegionRequest.BulkUpdate;

                //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).SetItemCountryRegionAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// Create Web Category
        /// </summary>
        /// <param name="createWebCategoryRequest"></param>
        public async Task<CreateWebCategoryResponse> CreateWebCategory(CreateWebCategoryRequest createWebCategoryRequest)
        {
            var res = new CreateWebCategoryResponse();
            try
            {
                var req = new CreateWebCategoryRequest();
                req.WebID = createWebCategoryRequest.WebID;                  //WebID used in content engine
                req.ParentID = createWebCategoryRequest.ParentID;               //Parent category ID (use 0 for the root)
                req.Description = createWebCategoryRequest.Description;

                // Send Request to Server and Get Response
                res = await _exigoApiContext.GetContext(false).CreateWebCategoryAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Update Web Category
        /// </summary>
        /// <param name="updateWebCategoryRequest"></param>
        public async Task<UpdateWebCategoryResponse> UpdateWebCategory(UpdateWebCategoryRequest updateWebCategoryRequest)
        {
            var res = new UpdateWebCategoryResponse();
            try
            {
                var req = new UpdateWebCategoryRequest();
                req.WebID = updateWebCategoryRequest.WebID;                  //WebID used in content engine
                req.CategoryID = updateWebCategoryRequest.CategoryID;
                req.Description = updateWebCategoryRequest.Description;          //Maximum length is 50

                //Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).UpdateWebCategoryAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Delete Web Category
        /// </summary>
        /// <param name="deleteWebCategoryRequest"></param>
        public async Task<DeleteWebCategoryResponse> DeleteWebCategory(DeleteWebCategoryRequest deleteWebCategoryRequest)
        {
            var res = new DeleteWebCategoryResponse();
            try
            {
                var req = new DeleteWebCategoryRequest();
                req.WebID = deleteWebCategoryRequest.WebID;                  //WebID used in content engine
                req.CategoryID = deleteWebCategoryRequest.CategoryID;

                // Send Request to Server and Get Response
                res = await _exigoApiContext.GetContext(false).DeleteWebCategoryAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Adjust Inventory
        /// </summary>
        /// <param name="adjustInventoryRequest"></param>
        public async Task<AdjustInventoryResponse> AdjustInventory(AdjustInventoryRequest adjustInventoryRequest)
        {
            var res = new AdjustInventoryResponse();
            try
            {
                var req = new AdjustInventoryRequest();
                req.WarehouseID = adjustInventoryRequest.WarehouseID;            //Unique location for orders
                req.ItemCode = adjustInventoryRequest.ItemCode;
                req.Quantity = adjustInventoryRequest.Quantity;
                req.Notes = adjustInventoryRequest.Notes;

                // Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).AdjustInventoryAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Set Item Subscription
        /// </summary>
        /// <param name="setItemSubscriptionRequest"></param>
        public async Task<SetItemSubscriptionResponse> SetItemSubscription(SetItemSubscriptionRequest setItemSubscriptionRequest)
        {
            var res = new SetItemSubscriptionResponse();
            try
            {
                var req = new SetItemSubscriptionRequest();
                req.ItemID = setItemSubscriptionRequest.ItemID;
                req.ItemCode = setItemSubscriptionRequest.ItemCode;
                req.Subscriptions = setItemSubscriptionRequest.Subscriptions;

                // Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).SetItemSubscriptionAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Set Item Point Account
        /// </summary>
        /// <param name="setItemPointAccountRequest"></param>
        public async Task<SetItemPointAccountResponse> SetItemPointAccount(SetItemPointAccountRequest setItemPointAccountRequest)
        {
            var res = new SetItemPointAccountResponse();
            try
            {
                var req = new SetItemPointAccountRequest();
                req.ItemID = setItemPointAccountRequest.ItemID;
                req.ItemCode = setItemPointAccountRequest.ItemCode;
                req.PointAccounts = setItemPointAccountRequest.PointAccounts;

                // Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).SetItemPointAccountAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        public async Task<GetPartiesResponse> GetParty(int partyId)
        {
            var req = new GetPartiesRequest();
            req.PartyID = partyId;
            var response = await _exigoApiContext.GetContext(false).GetPartiesAsync(req);
            return response;
        }

        public async Task<CreatePartyResponse> CreateParty(CreatePartyRequest createPartyRequest)
        {
            var res = new CreatePartyResponse();
            try
            {
                var req = new CreatePartyRequest();
                req.PartyType = 1;
                req.PartyStatusType = 1;
                req.HostID = 1;
                req.DistributorID = 1;
                req.StartDate = DateTime.Today;
                req.CloseDate = DateTime.Today;
                req.Description = "1";
                req.EventStart = DateTime.Today;
                req.EventEnd = DateTime.Today;
                req.LanguageID = 1;
                req.Information = "1";
                req.BookingPartyID = 1;
                req.Field1 = "1";
                req.Field2 = "1";
                req.Field3 = "1";
                req.Field4 = "1";
                req.Field5 = "1";
                // Send Request to Server and Get Response

                res = await _exigoApiContext.GetContext(false).CreatePartyAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        public async Task<GetCustomersResponse> GetCustomer(int customerID)
        {
            var req = new GetCustomersRequest();
            req.CustomerID = customerID;
            var response = await _exigoApiContext.GetContext(false).GetCustomersAsync(req);
            return response;
        }

        public async Task<GetOrdersResponse> GetCustomerReviewOrder(int customerId, int OrderId)
        {
            var req = new GetOrdersRequest();
            req.CustomerID = customerId;
            req.OrderID = 268403;
            var response = await _exigoApiContext.GetContext(false).GetOrdersAsync(req);
           
            int shipMethodId=response.Orders[0].ShipMethodID;

            var responseShipmethod= GetShipMethodsRequest(shipMethodId,response.Orders[0].WarehouseID);
            var currentShipmethodId = responseShipmethod.Where(x => x.ShipMethodID == shipMethodId).ToList();
            response.Orders[0].Other11 = currentShipmethodId.ToString();
            return response;
        }
        private List<ShipMethodsResponse> GetShipMethodsRequest(int shipMethodId, int warehouseId)
        {
            var shipMethods = new List<ShipMethodsResponse>();
                using (var sql = Common.Utils.DbConnection.Sql())
                {
                shipMethods = sql.Query<ShipMethodsResponse>(@"
                                        SELECT 
                                            [ShipMethodID]
                                            ,[ShipMethodDescription]
                                            ,[WarehouseID]
                                            ,[ShipCarrierID]
                                            ,[DisplayOnWeb]
                                        FROM [dbo].[ShipMethods]
                                        WHERE DisplayOnWeb = 1 
                                            AND WarehouseID = @wid",
                                        new
                                        {
                                            wid = warehouseId
                                        }).ToList();
                }

            return shipMethods;
        }

        public CustomerPointAccount GetCustomerLoyaltyPointAccount(int customerId, int LoyaltyPointAccountId)
        {
                var pointAccount = new CustomerPointAccount();
                using (var context = DbConnection.Sql())
                {
                    pointAccount = context.Query<CustomerPointAccount>(@"
                                SELECT cpa.PointAccountID
                                      , cpa.CustomerID
                                      , cpa.PointBalance AS Balance
	                                  , pa.PointAccountDescription
                                      , pa.CurrencyCode
                                FROM CustomerPointAccounts cpa
                                 LEFT JOIN PointAccounts pa
	                                ON cpa.PointAccountID = pa.PointAccountID
                                WHERE cpa.CustomerID = @CustomerID
                                    AND cpa.PointAccountID = @PointAccountID
                    ", new
                    {
                        CustomerID = customerId,
                        PointAccountID = LoyaltyPointAccountId
                    }).FirstOrDefault();
                }

                if (pointAccount == null) return null;

                return pointAccount;
        }

        

        /// <summary>
        /// DeleteCustomerAddress
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="type"></param>
        public async Task<Address> DeleteCustomerAddress(int customerID, Address address)
        {
            var type = address.AddressType;
            var deleteAddress = false;
            var request = new UpdateCustomerRequest();
            request.CustomerID = customerID;
            // Main address
            if (type == AddressType.Main)
            {
                deleteAddress = true;
                request.MainAddress1 = string.Empty;
                request.MainAddress2 = string.Empty;
                request.MainCity = string.Empty;
                request.MainState = string.Empty;
                request.MainZip = string.Empty;
                request.MainCountry = string.Empty;
            }
            // Mailing address
            else if (type == AddressType.Mailing)
            {
                deleteAddress = true;
                request.MailAddress1 = string.Empty;
                request.MailAddress2 = string.Empty;
                request.MailCity = string.Empty;
                request.MailState = string.Empty;
                request.MailZip = string.Empty;
                request.MailCountry = string.Empty;
            }
            // Other address
            else if (type == AddressType.Other)
            {
                deleteAddress = true;
                request.OtherAddress1 = string.Empty;
                request.OtherAddress2 = string.Empty;
                request.OtherCity = string.Empty;
                request.OtherState = string.Empty;
                request.OtherZip = string.Empty;
                request.OtherCountry = string.Empty;
            }
            if (deleteAddress)
            {

                await _exigoApiContext.GetContext(false).UpdateCustomerAsync(request);
            }
            return address;
        }
      
        public async Task<Address> SetCustomerAddressOnFile(int customerID, Address address)
        {
            return await SetCustomerAddressOnFile(customerID, address, address.AddressType);
        }
        public async Task<Address> SetCustomerAddressOnFile(int customerID, Address address, AddressType type)
        {
            var saveAddress = false;
            var request = new UpdateCustomerRequest();
            request.CustomerID = customerID; // Attempt to validate the user's entered address if US address
           // address = await _authenticateService.ValidateAddress(address) as Address; // New Addresses
            if (type == AddressType.New)
            {
                return await SaveNewCustomerAddress(customerID, address);
            } // Main address
            if (type == AddressType.Main)
            {
                saveAddress = true;
                request.MainAddress1 = address.Address1;
                request.MainAddress2 = address.Address2 ?? string.Empty;
                request.MainCity = address.City;
                request.MainState = address.State;
                request.MainZip = address.Zip;
                request.MainCountry = address.Country;
            } // Mailing address
            if (type == AddressType.Mailing)
            {
                saveAddress = true;
                request.MailAddress1 = address.Address1;
                request.MailAddress2 = address.Address2 ?? string.Empty;
                request.MailCity = address.City;
                request.MailState = address.State;
                request.MailZip = address.Zip;
                request.MailCountry = address.Country;
            } // Other address
            if (type == AddressType.Other)
            {
                saveAddress = true;
                request.OtherAddress1 = address.Address1;
                request.OtherAddress2 = address.Address2 ?? string.Empty;
                request.OtherCity = address.City;
                request.OtherState = address.State;
                request.OtherZip = address.Zip;
                request.OtherCountry = address.Country;
            }
            if (saveAddress)
            {
                await _exigoApiContext.GetContext(false).UpdateCustomerAsync(request);
            }
            return address;
        }
        public async Task SetCustomerPrimaryAddress(int customerID, AddressType type)
        {
            if (type == AddressType.Main || type == AddressType.New) return; var addressesOnFile = GetCustomerAddress(customerID)
            .Where(c => c.IsComplete); var oldPrimaryAddress = addressesOnFile
            .Where(c => c.AddressType == AddressType.Main)
            .FirstOrDefault(); var newPrimaryAddress = addressesOnFile
            .Where(c => c.AddressType == type)
            .FirstOrDefault(); if (oldPrimaryAddress == null || newPrimaryAddress == null) return; // Swap the addresses
            await SetCustomerAddressOnFile(customerID, (Address)newPrimaryAddress, AddressType.Main);
            await SetCustomerAddressOnFile(customerID, (Address)oldPrimaryAddress, type);
        }
        public async Task<Address> AddUpdateCustomerAddress(int customerID, Address address)
        {
            var type = address.AddressType;
            var saveAddress = false;
            var request = new UpdateCustomerRequest();
            request.CustomerID = customerID;



            // Attempt to validate the user's entered address if US address
            //address = GlobalUtilities.ValidateAddress(address) as Address;
            //exigoApiClient




            // New Addresses
            if (type == AddressType.New)
            {
                return await SaveNewCustomerAddress(customerID, address);
            }



            // Main address
            if (type == AddressType.Main)
            {
                saveAddress = true;
                request.MainAddress1 = address.Address1;
                request.MainAddress2 = address.Address2 ?? string.Empty;
                request.MainCity = address.City;
                request.MainState = address.State;
                request.MainZip = address.Zip;
                request.MainCountry = address.Country;
            }



            // Mailing address
            if (type == AddressType.Mailing)
            {
                saveAddress = true;
                request.MailAddress1 = address.Address1;
                request.MailAddress2 = address.Address2 ?? string.Empty;
                request.MailCity = address.City;
                request.MailState = address.State;
                request.MailZip = address.Zip;
                request.MailCountry = address.Country;
            }



            // Other address
            if (type == AddressType.Other)
            {
                saveAddress = true;
                request.OtherAddress1 = address.Address1;
                request.OtherAddress2 = address.Address2 ?? string.Empty;
                request.OtherCity = address.City;
                request.OtherState = address.State;
                request.OtherZip = address.Zip;
                request.OtherCountry = address.Country;
            }
            if (saveAddress)
            {
                await _exigoApiContext.GetContext(false).UpdateCustomerAsync(request);
            }
            return address;
        }
    }
}
