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
using ShippingAddress = WinkNatural.Web.Services.DTO.Shopping.ShippingAddress;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : BaseController
    {
        private readonly IShoppingService _shoppingService;
        private readonly IPropertyBags _propertyBagService;
        private readonly IPropertyBagItem _propertyBagItem;
        private readonly IOrderConfiguration _orderConfiguration;
        private readonly IMapper _mapper;
        private readonly IOptions<ConfigSettings> _config;
        private readonly ISqlCacheService _sqlCacheService;
        private readonly IGetCurrentMarket _getCurrentMarket;
        private readonly ICustomerPointAccount _customerPointAccount;
        private readonly IAutoOrders _autoOrders;
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IDistributedCache _distributedCache;
        private readonly IConfiguration _configuration;

        public IOrderConfiguration OrderConfiguration { get; set; }
        public IOrderConfiguration AutoOrderConfiguration { get; set; }
        public ShoppingController(IShoppingService shoppingService, IMapper mapper, IOptions<ConfigSettings> config, ISqlCacheService sqlCacheService, IPropertyBags propertyBagService, IPropertyBagItem propertyBagItem, IOrderConfiguration orderConfiguration, 
            IGetCurrentMarket getCurrentMarket, IConfiguration configuration, ICustomerPointAccount customerPointAccount,IAutoOrders autoOrders, IDistributedCache distributedCache, IExigoApiContext exigoApiContext)
        {
            _shoppingService = shoppingService;
            _mapper = mapper;
            _config = config;
            _sqlCacheService = sqlCacheService;
            _propertyBagService = propertyBagService;
            _propertyBagItem = propertyBagItem;
            _orderConfiguration = orderConfiguration;
            _getCurrentMarket = getCurrentMarket;
            _customerPointAccount = customerPointAccount;
            _autoOrders = autoOrders;
            _exigoApiContext = exigoApiContext;
            _distributedCache = distributedCache;
            _configuration = configuration;

        }
                
        public ShoppingCartItemsPropertyBag ShoppingCart
        {
            get
            {
                if (_shoppingCart == null)
                {
              //      _shoppingCart = _propertyBagService.GetCacheData<ShoppingCartItemsPropertyBag>(_config.Value.Globalization.CookieKey + "ReplicatedSiteShopping" + "Cart");
                }
                return _shoppingCart;
            }
        }
        public ShoppingCartCheckoutPropertyBag PropertyBag
        {
            get
            {
                if (_propertyBag == null)
                {
                 //   _propertyBag = _propertyBagService.GetCacheData<ShoppingCartCheckoutPropertyBag>(_config.Value.Globalization.CookieKey + "ReplicatedSiteShopping" + "PropertyBag");
                }
                return _propertyBag;
            }
        }
        private ShoppingCartCheckoutPropertyBag _propertyBag;
        private ShoppingCartItemsPropertyBag _shoppingCart;

        public Exception PointPaymentError = new Exception(Resources.CommonResource.PointPaymentError);

        /// <summary>
        /// Get item category
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetItemCategory/{webCategoryID:int}")]
        public IActionResult GetItemCategory(int webCategoryID)
        {
            return Ok(_shoppingService.GetItemCategory(webCategoryID));
        }

        /// <summary>
        /// GetProductList by categoryID
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetProductList/{categoryID:int}/{sortBy:int}")]
        public IActionResult GetProductList(int categoryID, int sortBy)
        {
            return Ok(_shoppingService.GetShopProducts(categoryID, sortBy));
        }

        /// <summary>
        /// GetProductDetailById by itemCode
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetProductDetailById/{itemCode}")]///{itemCode:string}
        public IActionResult GetProductDetailById(string itemCode)
        {
            string[] itemCodes = new string[1];
            itemCodes[0] = itemCode;
            return Ok(_shoppingService.GetProductDetailById(itemCodes));
        }

        /// <summary>
        /// GetProductImage by imageName
        /// </summary>
        /// <returns></returns>
        [HttpGet("ProductImage/{imageName}")]
        public IActionResult GetProductImage(string imageName)
        {
            try
            {
                var imageResponse = _shoppingService.GetProductImage(imageName);
                return File(imageResponse, "image/jpeg");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        //[Route("GetCartItem")]
        //[HttpGet]
        //public IActionResult GetCartItem()
        //{
        //    var n = _propertyBagService.GetCacheData("39816a50-159a-46a7-a576-e3df026f5ab8");
        //    return Ok(n);
        //}
        //[Route("RemoveCartItem")]
        //[HttpGet]
        //public IActionResult RemoveCartItem()
        //{
        //     _propertyBagService.DeleteCacheCartData("42af4af7-1d8d-40a0-a421-0739272d2f3a");
        //    return Ok();
        //}

        [HttpPost("AddToCart")]
        public IActionResult AddToCart(Item item)
        {
            var braintreeService = new BraintreeService(_config);
            var PaypalClientToken = braintreeService.GetClientToken(); //This token will be needed in the paypal.cshtml
            var CountryCode = "US";
            OrderConfiguration = _getCurrentMarket.curretMarket(CountryCode).GetConfiguration().Orders;

            bool checkQuantity = ShoppingCart.Items.CheckItemBackOffice(item);
            if (!checkQuantity)
            {
                return Ok();
            }

            //  ShoppingCart.Items.Add(item);
            //  ShoppingCartItem itms = new ShoppingCartItem(item);
            ShoppingCart.Items.Add(item);
            // _propertyBagService.UpdateCacheData(ShoppingCart);
            //  _propertyBagService.UpdateCacheData(ShoppingCart);


            var languageID = Language.GetSelectedLanguageID();
            var cartItems = ShoppingCart.Items.ToList();


            var items = _propertyBagItem.GetItems(cartItems, OrderConfiguration, languageID).ToList();
            var cookie = Request.Cookies[_config.Value.Globalization.CookieKey + "UserID"];

            var dripItems = new List<Items>();
            foreach (var itm in items)
            {
                dripItems.Add(new Items
                {
                    product_id = itm.ItemID.ToString(),
                    name = itm.ItemDescription,
                    price = itm.Price,
                    quantity = itm.Quantity,
                    sku = itm.ItemCode,
                    image_url = new Uri(_config.Value.Company.BaseReplicatedUrl + itm.LargeImageUrl).AbsoluteUri
                });
            }

            var dripMail = new DripMail();
            dripMail.Enqueue(new CartDripData
            {

                Type = 1,
                provider = "Wink Exigo",
                person_id = 1,//Identity.CustomerID,
                email = "abc@gmail.com",// Identity.Email==null?"": Identity.Email,
                action = dripItems.Count() > 1 ? "updated" : "created",
                cart_id = ShoppingCart.SessionID,
                cart_url = "https://winknaturals.com/WinkCorporate/store/cart",
                items = dripItems
            }); ;

            return new JsonResult(new
            {
                success = true,
                cartitems = items,
                userid = 1,//Identity.CustomerID,
                email = "abc@gmail.com",//Identity.Email,
                baseurl = _config.Value.Globalization.BaseImageURL,

            });
        }

        /// <summary>
        /// SubmitCheckout
        /// </summary>
        /// <returns></returns>
        [HttpPost("SubmitCheckout")]
        public IActionResult SubmitCheckout(TransactionalRequestModel transactionRequests)
        {          
            return Ok(_shoppingService.SubmitCheckout(transactionRequests, Identity.CustomerID));
        }

        //if (!PropertyBag.IsSubmitting)
        //{
        //    PropertyBag.IsSubmitting = true;
        // //   _propertyBag = _propertyBagService.UpdateCacheData(PropertyBag);

        //    try
        //    {
        //        // Start creating the API requests
        //        var willCallAddress = PropertyBag.ShipMethodID == 9 ? PropertyBag.WillCallShippingAddress : PropertyBag.ShippingAddress;
        //       var details = new List<ApiRequest>();
        //        var orderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order);
        //        var hasOrder = orderItems.Count() > 0;
        //        var autoOrderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder);
        //        var hasAutoOrder = autoOrderItems.Count() > 0;
        //        var makePostTransactionPointPayment = false;
        //        var pointAccount = new CustomerPointAccount();
        //        var address = new Infrastructure.Services.ExigoService.ShippingAddress();
        //        var pointPaymentAmount = 0m;
        //        var autoOrderPaymentType = new AutoOrderPaymentType();

        //        if (hasOrder)
        //        {
        //            //OrderConfiguration, PropertyBag.ShipMethodID, orderItems, willCallAddress
        //            var orderRequest = new CreateOrderRequest()
        //            {

        //                WarehouseID = OrderConfiguration.WarehouseID,
        //                PriceType = OrderConfiguration.PriceTypeID,
        //                CurrencyCode = OrderConfiguration.CurrencyCode,
        //                OrderDate = DateTime.Now,
        //                OrderType = OrderType.ShoppingCart,
        //                OrderStatus = OrderStatusType.Incomplete,
        //                ShipMethodID = PropertyBag.ShipMethodID,   
        //                Details = (OrderDetailRequest[])orderItems.Select(c => (OrderDetailRequest)(c as ShoppingCartItem)).ToArray(),

        //                FirstName = address.FirstName,
        //                MiddleName = address.MiddleName,
        //                LastName = address.LastName,
        //                Email = address.Email,
        //                Phone = address.Phone,
        //                Address1 = address.Address1,
        //                Address2 = address.Address2,
        //                City = address.City,
        //                State = address.State,
        //                Zip = address.Zip,
        //                Country = address.Country,

        //                CustomerID = Identity.CustomerID,
        //                Other17 = PropertyBag.QuantityOfPointsToUse.ToString() // Points
        //            };

        //            if (PropertyBag.Coupon != null && String.IsNullOrEmpty(PropertyBag.Coupon.Code))
        //            {
        //                orderRequest.Other16 = PropertyBag.Coupon.Code;
        //            }
        //            if (PropertyBag.ContainsSpecial)
        //            {
        //                orderRequest.Other18 = "true";
        //            }
        //            details.Add(orderRequest);
        //        }

        //        if (hasAutoOrder)
        //        {
        //            // CreateAutoOrderRequest(AutoOrderConfiguration, DAL.GetAutoOrderPaymentType(PropertyBag.PaymentMethod), PropertyBag.AutoOrderStartDate, 8, autoOrderItems, PropertyBag.ShippingAddress)
        //            var autoOrderRequest = new CreateAutoOrderRequest()
        //            {
        //                CustomerID = Identity.CustomerID,
        //                WarehouseID = OrderConfiguration.WarehouseID,
        //                PriceType = OrderConfiguration.PriceTypeID,
        //                CurrencyCode = OrderConfiguration.CurrencyCode,
        //                StartDate = DateTime.Now,
        //                PaymentType = autoOrderPaymentType,
        //                ProcessType = AutoOrderProcessType.AlwaysProcess,
        //                ShipMethodID = PropertyBag.ShipMethodID,
        //                Details = (OrderDetailRequest[])orderItems.Select(c => (OrderDetailRequest)(c as ShoppingCartItem)).ToArray(),
        //                Frequency = PropertyBag.AutoOrderFrequencyType,

        //                FirstName = address.FirstName,
        //                MiddleName = address.MiddleName,
        //                LastName = address.LastName,
        //                Email = address.Email,
        //                Phone = address.Phone,
        //                Address1 = address.Address1,
        //                Address2 = address.Address2,
        //                City = address.City,
        //                State = address.State,
        //                Zip = address.Zip,
        //                Country = address.Country,

        //            };

        //            var getItemRequest = new GetItemsRequest();
        //            {
        //                getItemRequest.Configuration = AutoOrderConfiguration;
        //                getItemRequest.ItemCodes = autoOrderRequest.Details.Select(i => i.ItemCode).ToArray();
        //            }
        //            // Keep prices as they were when creating autoorder
        //            var items1 = _propertyBagItem.GetShoppingCartItem(getItemRequest).ToList();
        //            //  {
        //            //     Configuration = AutoOrderConfiguration,
        //            //    ItemCodes = autoOrderRequest.Details.Select(i => i.ItemCode).ToArray(),
        //            // }).ToList();


        //            foreach (var itm in autoOrderRequest.Details)
        //            {
        //                itm.PriceEachOverride = items1.Where(y => y.ItemCode == itm.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                itm.TaxableEachOverride = items1.Where(y => y.ItemCode == itm.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                itm.ShippingPriceEachOverride = items1.Where(y => y.ItemCode == itm.ItemCode).Select(y => y.Price).FirstOrDefault();
        //                itm.BusinessVolumeEachOverride = items1.Where(y => y.ItemCode == itm.ItemCode).Select(y => y.BV).FirstOrDefault();
        //                itm.CommissionableVolumeEachOverride = items1.Where(y => y.ItemCode == itm.ItemCode).Select(y => y.CV).FirstOrDefault();
        //            }
        //            details.Add(autoOrderRequest);
        //            //
        //            if (Identity.CustomerID == CustomerTypes.RetailCustomer)
        //            {

        //                var updateCustomerRequest = new UpdateCustomerRequest
        //                {
        //                    CustomerID = Identity.CustomerID,
        //                    CustomerType = CustomerTypes.PreferredCustomer,
        //                    Field1 = hasAutoOrder ? "1" : ""
        //                };
        //                details.Add(updateCustomerRequest);
        //            }
        //        }

        //        var remainder = 0m;

        //        #region Point Account Validation Logic

        //        if (PropertyBag.UsePointsAsPayment)
        //        {
        //            var orderCalcRequest = new CalculateOrderRequest()
        //            {

        //                //Configuration = OrderConfiguration,
        //                // Items = orderItems,
        //                //Address = (IAddress)PropertyBag.ShippingAddress,
        //                ShipMethodID = PropertyBag.ShipMethodID,

        //                CustomerID = Identity.CustomerID,
        //                Other17 = PropertyBag.QuantityOfPointsToUse.ToString() // Points
        //            };

        //            if (PropertyBag.Coupon != null && String.IsNullOrEmpty(PropertyBag.Coupon.Code))
        //            {
        //                orderCalcRequest.Other16 = PropertyBag.Coupon.Code;
        //            }
        //            if (PropertyBag.ContainsSpecial)
        //            {
        //                orderCalcRequest.Other18 = "true";
        //            }


        //            var orderTotals = await _shoppingService.CalculateOrder(orderCalcRequest);

        //            pointPaymentAmount = orderTotals.SubTotal < PropertyBag.QuantityOfPointsToUse ? orderTotals.SubTotal : PropertyBag.QuantityOfPointsToUse;
        //            remainder = orderTotals.Total - pointPaymentAmount;
        //            pointAccount = (CustomerPointAccount)_customerPointAccount.GetCustomerPointAccounts(Identity.CustomerID, 1);//GetCustomerPointAccount(Identity.Customer.CustomerID, PointAccounts.LoyaltyPointAccount);

        //            if (pointAccount != null && pointAccount.Balance > 0)
        //            {
        //                var haveEnoughPoints = decimal.Round(pointAccount.Balance, 2) >= decimal.Round(pointPaymentAmount, 2);

        //                if (haveEnoughPoints)
        //                {
        //                    if (remainder > 0)
        //                    {
        //                        if (PropertyBag.PaymentMethod == null)
        //                            throw PointPaymentError;
        //                        // make a post trans point request
        //                        // but for now create their cc/bank account request and add to details
        //                        if (PropertyBag.PaymentMethod is CreditCard)
        //                        {
        //                            var card = PropertyBag.PaymentMethod as CreditCard;

        //                            if (card.Type == CreditCardType.New)
        //                            {
        //                                if (hasAutoOrder)
        //                                {
        //                                    card = (CreditCard)_customerPointAccount.SaveNewCustomerCreditCard(1, card);//DAL.SaveNewCustomerCreditCard(1, card);
        //                                    ((CreateAutoOrderRequest)details.Where(c => c is CreateAutoOrderRequest).FirstOrDefault()).PaymentType = (AutoOrderPaymentType)_autoOrders.GetAutoOrderPaymentType(card);//DAL.GetAutoOrderPaymentType(card);
        //                                }
        //                                if (hasOrder)
        //                                {
        //                                    if (!card.IsTestCreditCard)
        //                                    {
        //                                        var ccctRequest = new Exigo.Api.Client.ChargeCreditCardTokenRequest();
        //                                        ccctRequest.MaxAmount = remainder;
        //                                        details.Add(ccctRequest);
        //                                    }
        //                                    else
        //                                    {
        //                                        // Test Credit Card, so no need to charge card
        //                                        ((CreateOrderRequest)details.Where(c => c is CreateOrderRequest).FirstOrDefault()).OrderStatus = (OrderStatusType)GlobalUtilities.GetDefaultOrderStatusType();
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (hasOrder)
        //                                {
        //                                    var cctofRequest = new ChargeCreditCardTokenOnFileRequest();
        //                                    cctofRequest.MaxAmount = remainder;
        //                                    details.Add(cctofRequest);
        //                                }
        //                            }
        //                        }

        //                        makePostTransactionPointPayment = true;
        //                    }
        //                    else
        //                    {
        //                        var pointPaymentRequest = new CreatePaymentPointAccountRequest
        //                        {
        //                            //default value to be set
        //                            PointAccountID = 1,
        //                            PaymentDate = DateTime.Now.ToCST(),
        //                            Amount = pointPaymentAmount
        //                        };

        //                        details.Add(pointPaymentRequest);
        //                    }
        //                }
        //                else
        //                {
        //                    throw PointPaymentError;
        //                }
        //            }




        //            else
        //            {
        //                // Somehow the session got marked at UsePointsForPayment but the Point Account is null, so something went wrong and we need to let the user know they have to enter a new Payment Method.
        //                PropertyBag.UsePointsAsPayment = false;
        //              //  _propertyBagService.UpdateCacheData(PropertyBag);
        //                throw new Exception(string.Format(Resources.CommonResource.PointPaymentError3, _config.Value.Company.Phone));
        //            }
        //        }
        //        #endregion

        //        // Create the payment request
        //        else if (PropertyBag.PaymentMethod is CreditCard)
        //        {
        //            var card = PropertyBag.PaymentMethod as CreditCard;

        //            if (card.Type == CreditCardType.New)
        //            {
        //                if (hasAutoOrder)
        //                {
        //                    // card = DAL.SaveNewCustomerCreditCard(1, card);
        //                    card = (CreditCard)_customerPointAccount.SaveNewCustomerCreditCard(Identity.CustomerID, card);
        //                    ((CreateAutoOrderRequest)details.Where(c => c is CreateAutoOrderRequest).FirstOrDefault()).PaymentType = (AutoOrderPaymentType)_autoOrders.GetAutoOrderPaymentType(card);
        //                }
        //                if (hasOrder)
        //                {
        //                    if (!card.IsTestCreditCard)
        //                    {
        //                        var ccctrequest = new ChargeCreditCardTokenRequest();
        //                        details.Add(ccctrequest);
        //                    }
        //                    else
        //                    {
        //                        // Test Credit Card, so no need to charge card
        //                        ((CreateOrderRequest)details.Where(c => c is CreateOrderRequest).FirstOrDefault()).OrderStatus = (OrderStatusType)GlobalUtilities.GetDefaultOrderStatusType();
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (hasOrder)
        //                {
        //                    var cctofrequest = new ChargeCreditCardTokenOnFileRequest();
        //                    details.Add(cctofrequest);
        //                }
        //            }
        //        }

        //        // Process the transaction
        //        var transactionRequest = new TransactionalRequest
        //        {
        //            TransactionRequests = (ITransactionMember[])details.ToArray()
        //        };
        //        var transactionResponse = _shoppingService.SubmitCheckout(transactionRequests);//ProcessTransaction(transactionRequest);
        //        var newOrderID = 0;
        //        var newAutoOrderID = 0;

        //        CreateOrderResponse orderResponse = null;
        //        CreateAutoOrderResponse autoOrderResponse = null;

        //        if (transactionResponse.Status == TaskStatus.Created)
        //        {
        //            foreach (var response in transactionResponse.Result.TransactionResponses)
        //            {

        //                if (response is CreateOrderResponse)
        //                {
        //                    newOrderID = ((CreateOrderResponse)response).OrderID;
        //                    orderResponse = (CreateOrderResponse)response;
        //                }
        //                if (response is CreateAutoOrderResponse)
        //                {
        //                    newAutoOrderID = ((CreateAutoOrderResponse)response).AutoOrderID;
        //                    autoOrderResponse = (CreateAutoOrderResponse)response;
        //                }

        //            }

        //            if (makePostTransactionPointPayment)
        //            {
        //                PayUsingPointAccount(newOrderID, pointPaymentAmount, pointAccount);
        //            }
        //        }

        //        PropertyBag.NewOrderID = newOrderID;
        //        PropertyBag.NewAutoOrderID = newAutoOrderID;
        //      //  _propertyBag = _propertyBagService.UpdateCacheData(PropertyBag);

        //        //Collecting items in the order to create order in YotPo for reviews
        //        var allOrderItems = orderItems.ToList();
        //        if (hasAutoOrder)
        //        {
        //            allOrderItems.AddRange(autoOrderItems);
        //        }

        //        var items = _propertyBagItem.GetShoppingCartItem(new GetItemsRequest
        //        {
        //            Configuration = OrderConfiguration,
        //            ItemCodes = allOrderItems.Select(i => i.ItemCode).ToArray(),
        //        }).ToList();

        //        PurchaseAdaptor purchaseAdaptor = new PurchaseAdaptor();
        //        var productBaseUrl = _config.Value.Globalization.ReplicatedSites + GetFormattedUrl("wwww") + "/store/products/)";
        //        var purchaseRequest = purchaseAdaptor.CreatePurchaseRequest(newOrderID.ToString(), Identity.FirstName, willCallAddress.Email, productBaseUrl, items);
        //        YotPoApiService yotPoApiService = new YotPoApiService();
        //        var yotPoresponse = yotPoApiService.PostOrder(purchaseRequest);

        //        if (newAutoOrderID > 0)
        //        {
        //            purchaseRequest = purchaseAdaptor.CreatePurchaseRequest(newAutoOrderID.ToString(), Identity.FirstName, willCallAddress.Email, productBaseUrl, items);
        //            yotPoApiService = new YotPoApiService();
        //            yotPoresponse = yotPoApiService.PostOrder(purchaseRequest);
        //        }
        //        var cookie = Request.Cookies[_config.Value.Globalization.CookieKey + "UserID"];

        //        //Send order data to drip 
        //        var dripItems = new List<Items>();
        //        foreach (var itm in items)
        //        {
        //            dripItems.Add(new Items
        //            {
        //                product_id = itm.ItemID.ToString(),
        //                name = itm.ItemDescription,
        //                price = itm.Price,
        //                quantity = itm.Quantity,
        //                sku = itm.ItemCode,
        //                image_url = new Uri(_config.Value.Company.BaseReplicatedUrl + itm.LargeImageUrl).AbsoluteUri
        //            });
        //        }

        //        var shipping_address = new Utilities.WebDrip.ShippingAddress
        //        {
        //            address_1 = PropertyBag.ShippingAddress.Address1 ?? "",
        //            address_2 = PropertyBag.ShippingAddress.Address2 ?? "",
        //            city = PropertyBag.ShippingAddress.City ?? "",
        //            state = PropertyBag.ShippingAddress.State ?? "",
        //            country = PropertyBag.ShippingAddress.Country ?? "",
        //            postal_code = PropertyBag.ShippingAddress.Zip ?? "",
        //            phone = PropertyBag.ShippingAddress.Phone ?? ""
        //        };

        //        var paymentAddress = ((CreditCard)PropertyBag.PaymentMethod).BillingAddress;
        //        var billing_address = new BillingAddress
        //        {
        //            address_1 = paymentAddress.Address1 ?? "",
        //            address_2 = paymentAddress.Address2 ?? "",
        //            city = paymentAddress.City ?? "",
        //            state = paymentAddress.State ?? "",
        //            country = paymentAddress.Country ?? "",
        //            postal_code = paymentAddress.Zip ?? ""
        //        };

        //        // item or Items : items = dripItems,
        //        var dripMail = new DripMail();
        //        dripMail.Enqueue(new OrderDripData
        //        {
        //            Type = 2,
        //            provider = "Wink DAL",
        //            person_id = Identity.CustomerID,
        //            email = Identity.Email,
        //            phone = Identity.Phone,
        //            action = "placed",
        //            items = dripItems,
        //            order_id = orderResponse?.OrderID.ToString(),
        //            grand_total = orderResponse?.Total,
        //            total_discounts = orderResponse?.DiscountTotal,
        //            total_shipping = orderResponse?.ShippingTotal,
        //            total_taxes = orderResponse?.TaxTotal,
        //            shipping_address = shipping_address,
        //            billing_address = billing_address
        //        });

        //        return new JsonResult(new
        //        {
        //            success = true,
        //            orderId = orderResponse?.OrderID,
        //            orderTotal = orderResponse?.Total,
        //            autoOrderId = autoOrderResponse?.AutoOrderID,
        //            autoOrderTotal = autoOrderResponse?.Total,
        //            shippingAddress = JsonConvert.SerializeObject(shipping_address),
        //            paymentAddress = JsonConvert.SerializeObject(billing_address),
        //            baseurl = _config.Value.Company.BaseReplicatedUrl,
        //            items = items,
        //            userid = Identity.CustomerID,//cookie.Value,
        //            tax = orderResponse?.TaxTotal,
        //            shipping = orderResponse?.ShippingTotal,
        //            discount = orderResponse?.DiscountTotal,
        //        });
        //    }
        //    catch (Exception exception)
        //    {
        //        PropertyBag.OrderException = exception.Message == "100001: Successful." ? "There was an issue with this transaction." : exception.Message.Replace("100001: Successful.", "");
        //        PropertyBag.IsSubmitting = false;
        //     //   _propertyBag = _propertyBagService.UpdateCacheData(PropertyBag);

        //        return new JsonResult(new
        //        {
        //            success = false,
        //            message = exception.Message == "100001: Successful." ? "There was an issue with this transaction." : exception.Message.Replace("100001: Successful.", "")
        //        });
        //    }
        //}
        //return Ok(_shoppingService.SubmitCheckout(transactionRequests));

        /// <summary>
        /// CalculateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("CalculateOrder")]
        public IActionResult CalculateOrder(CalculateOrderRequest calculateOrderReq, int shipMethodID = 0)
        {
            return Ok(_shoppingService.CalculateOrder(calculateOrderReq));
        }

        /// <summary>
        /// CreateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateOrder")]
        public IActionResult CreateOrder(CreateOrderRequest createOrderRequest)
        {
            return Ok(_shoppingService.CreateOrder(createOrderRequest));
        }

        /// <summary>
        /// CreateOrderImport
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateOrderImport")]
        public IActionResult CreateOrderImport(CreateOrderImportRequest createOrderImportRequest)
        {
            return Ok(_shoppingService.CreateOrderImport(createOrderImportRequest));
        }

        /// <summary>
        /// UpdateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("UpdateOrder")]
        public IActionResult UpdateOrder(UpdateOrderRequest updateOrderRequest)
        {
            return Ok(_shoppingService.UpdateOrder(updateOrderRequest));
        }

        /// <summary>
        /// ChangeOrderStatus
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangeOrderStatus")]
        public IActionResult ChangeOrderStatus(ChangeOrderStatusRequest changeOrderStatusRequest)
        {
            return Ok(_shoppingService.ChangeOrderStatus(changeOrderStatusRequest));
        }

        /// <summary>
        /// ChangeOrderStatusBatch
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangeOrderStatusBatch")]
        public IActionResult ChangeOrderStatusBatch(ChangeOrderStatusBatchRequest changeOrderStatusBatchRequest)
        {
            return Ok(_shoppingService.ChangeOrderStatusBatch(changeOrderStatusBatchRequest));
        }

        /// <summary>
        /// ValidateCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ValidateCreditCardToken")]
        public IActionResult ValidateCreditCardToken(ValidateCreditCardTokenRequest creditCardTokenRequest)
        {
            return Ok(_shoppingService.ValidateCreditCardToken(creditCardTokenRequest));
        }

        /// <summary>
        /// CreatePayment
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePayment")]
        public IActionResult CreatePayment(CreatePaymentRequest createPaymentRequest)
        {
            return Ok(_shoppingService.CreatePayment(createPaymentRequest));
        }

        /// <summary>
        /// CreatePaymentCreditCard
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentCreditCard")]
        public IActionResult CreatePaymentCreditCard(CreatePaymentCreditCardRequest createPaymentCreditCardRequest)
        {
            return Ok(_shoppingService.CreatePaymentCreditCard(createPaymentCreditCardRequest));
        }

        /// <summary>
        /// CreatePaymentWallet
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentWallet")]
        public IActionResult CreatePaymentWallet(CreatePaymentWalletRequest createPaymentWalletRequest)
        {
            return Ok(_shoppingService.CreatePaymentWallet(createPaymentWalletRequest));
        }

        /// <summary>
        /// CreatePaymentPointAccount
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentPointAccount")]
        public IActionResult CreatePaymentPointAccount(CreatePaymentPointAccountRequest createPaymentPointAccountRequest)
        {
            return Ok(_shoppingService.CreatePaymentPointAccount(createPaymentPointAccountRequest));
        }

        /// <summary>
        /// CreatePaymentCheck
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentCheck")]
        public IActionResult CreatePaymentCheck(CreatePaymentCheckRequest createPaymentCheckRequest)
        {
            return Ok(_shoppingService.CreatePaymentCheck(createPaymentCheckRequest));
        }

        /// <summary>
        /// ChargeCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeCreditCardToken")]
        public IActionResult ChargeCreditCardToken(ChargeCreditCardTokenRequest chargeCreditCardTokenRequest)
        {
            return Ok(_shoppingService.ChargeCreditCardToken(chargeCreditCardTokenRequest));
        }

        /// <summary>
        /// ChargeCreditCardTokenOnFile
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeCreditCardTokenOnFile")]
        public IActionResult ChargeCreditCardTokenOnFile(ChargeCreditCardTokenOnFileRequest chargeCreditCardTokenOnFileRequest)
        {
            return Ok(_shoppingService.ChargeCreditCardTokenOnFile(chargeCreditCardTokenOnFileRequest));
        }

        /// <summary>
        /// ChargeGroupOrderCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeGroupOrderCreditCardToken")]
        public IActionResult ChargeGroupOrderCreditCardToken(ChargeGroupOrderCreditCardTokenRequest chargeGroupOrderCredit)
        {
            return Ok(_shoppingService.ChargeGroupOrderCreditCardToken(chargeGroupOrderCredit));
        }

        /// <summary>
        /// RefundPriorCreditCardCharge
        /// </summary>
        /// <returns></returns>
        [HttpPost("RefundPriorCreditCardCharge")]
        public IActionResult RefundPriorCreditCardCharge(RefundPriorCreditCardChargeRequest refundPriorCredit)
        {
            return Ok(_shoppingService.RefundPriorCreditCardCharge(refundPriorCredit));
        }
        /// <summary>
        /// RefundPriorCreditCardCharge
        /// </summary>F
        /// <returns></returns>
        [HttpPost]
        [Route("checkout/shipping")]
        public IActionResult Shipping(VerifyAddressRequest addressRequest)
        {
            return Ok(_shoppingService.Shipping(addressRequest));
        }

        [HttpGet]
        [Route("GetshippingAddress/{CustomerID:int}")]
        public IActionResult GetCustomerAddress(int CustomerID)
        {
            return Ok(_shoppingService.GetCustomerAddress(CustomerID));
        }

        [HttpPost("AddUpdateCustomerAddress/{CustomerID:int}")]
        public IActionResult AddUpdateCustomerAddress(int CustomerID, ShippingAddress address)
        {
            //if ( Address.AddressType == AddressType.New)
            //{
            //    DAL.SetCustomerAddressOnFile(Identity.Customer.CustomerID, address as Address);
            //}
            return Ok(_shoppingService.AddUpdateCustomerAddress(CustomerID, address));

        }

        [HttpPost("GetWarehouses")]
        public IActionResult GetWarehouses(GetWarehousesRequest warehousesRequest)
        {
            return Ok(_shoppingService.GetWarehouses(warehousesRequest));
        }
      
  
        [HttpGet("SearchProducts/{query}")]
        public IActionResult SearchProducts(string query)
        {
            return Ok(_shoppingService.SearchProducts(query));
        }

        // To implement Special Item block in Cart
        [HttpGet("GetOrderSpecialItem")]
        public IActionResult GetOrderSpecialItem()
        {
            return Ok(_shoppingService.GetOrderSpecialItem());
        }

        [HttpGet("GetCustomerRealTime/{customerID:int}")]
        //To Get customer detail for editing.
        public IActionResult GetCustomerRealTime(int customerID)
        {
            return Ok(_shoppingService.GetCustomerRealTime(customerID));
        }

        //To update customer detail
        [HttpPost("UpdateCustomer")]
        public IActionResult UpdateCustomer(UpdateCustomerRequest updateCustomerRequest)
        {
            return Ok(_shoppingService.UpdateCustomer(updateCustomerRequest));
        }

        //Apply promocode
        [HttpGet("PromoCode/{promoCode}")]
        public IActionResult GetPromoCode(string promoCode)
        {
            return Ok(_shoppingService.GetPromoDetail(promoCode, Identity.CustomerID));
        }

        /// <summary>
        /// To Create Item
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateItem")]
        public IActionResult CreateItem(CreateItemRequest createItemRequest)
        {
            return Ok(_shoppingService.CreateItem(createItemRequest));
        }

        /// <summary>
        /// To Update Item
        /// </summary>
        /// <returns></returns>

        [HttpPost("UpdateItem")]
        public IActionResult UpdateItem(UpdateItemRequest updateItemRequest)
        {
            return Ok(_shoppingService.UpdateItem(updateItemRequest));
        }

        /// <summary>
        /// To Set Item Price
        /// </summary>
        /// <returns></returns>

        [HttpPost("SetItemPrice")]
        public IActionResult SetItemPrice(SetItemPriceRequest setItemPriceRequest)
        {
            return Ok(_shoppingService.SetItemPrice(setItemPriceRequest));
        }

        /// <summary>
        /// To Set Item Warehouse
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetItemWarehouse")]
        public IActionResult SetItemWarehouse(SetItemWarehouseRequest setItemWarehouseRequest)
        {
            return Ok(_shoppingService.SetItemWarehouse(setItemWarehouseRequest));
        }

        /// <summary>
        /// To Set Item CountryRegion
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetItemCountryRegion")]
        public IActionResult SetItemCountryRegion(SetItemCountryRegionRequest setItemWarehouseRequest)
        {
            return Ok(_shoppingService.SetItemCountryRegion(setItemWarehouseRequest));
        }

        /// <summary>
        /// To Create WebCategory
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateWebCategory")]
        public IActionResult CreateWebCategory(CreateWebCategoryRequest createWebCategoryRequest)
        {
            return Ok(_shoppingService.CreateWebCategory(createWebCategoryRequest));
        }

        /// <summary>
        /// To Update WebCategory
        /// </summary>
        /// <returns></returns>
        [HttpPost("UpdateWebCategory")]
        public IActionResult UpdateWebCategory(UpdateWebCategoryRequest updateWebCategoryRequest)
        {
            return Ok(_shoppingService.UpdateWebCategory(updateWebCategoryRequest));
        }

        // To Delete WebCategory
        [HttpPost("DeleteWebCategory")]
        public IActionResult DeleteWebCategory(DeleteWebCategoryRequest deleteWebCategoryRequest)
        {
            return Ok(_shoppingService.DeleteWebCategory(deleteWebCategoryRequest));
        }

        // To adjust inventory
        [HttpPost("AdjustInventory")]
        public IActionResult AdjustInventory(AdjustInventoryRequest adjustInventoryRequest)
        {
            return Ok(_shoppingService.AdjustInventory(adjustInventoryRequest));
        }

        // To set Item subscription
        [HttpPost("SetItemSubscription")]
        public IActionResult SetItemSubscription(SetItemSubscriptionRequest setItemSubscriptionRequest)
        {
            return Ok(_shoppingService.SetItemSubscription(setItemSubscriptionRequest));
        }

        // To Set Item Point Account
        [HttpPost("SetItemPointAccount")]
        public IActionResult SetItemPointAccount(SetItemPointAccountRequest setItemPointAccountRequest)
        {
            return Ok(_shoppingService.SetItemPointAccount(setItemPointAccountRequest));
        }

        [HttpPost("AddSpecialItemToCart")]
        public IActionResult AddSpecialItemToCart(Item item)
        {
            if (!string.IsNullOrEmpty(item.Field5))
            {
                item.PriceEachOverride = decimal.Parse(item.Field5);
                // ShoppingCart.Items.Add(item);
                PropertyBag.ContainsSpecial = true;
                 _propertyBagService.UpdateCacheData(ShoppingCart);
            }
            return Ok();
        }

        [HttpPost("RemoveItemFromCart")]
        public IActionResult RemoveItemFromCart(Guid id)
        {
            var item = ShoppingCart.Items.Where(c => c.ID == id).FirstOrDefault();
            var subtotal = 0M;
            var itemType = item.Type;
            if (!string.IsNullOrEmpty(item.Field5))
            {
                PropertyBag.ContainsSpecial = false;
                  _propertyBagService.UpdateCacheData(PropertyBag);
                // PropertyBags.Update(PropertyBag);
            }
            ShoppingCart.Items.Remove(id);
            if (ShoppingCart.Items.Count() == 1 && !string.IsNullOrEmpty(ShoppingCart.Items.First().Field5))
            {
                ShoppingCart.Items.Remove(ShoppingCart.Items.First().ID);
                PropertyBag.ContainsSpecial = false;
                //   _propertyBagService.UpdateCacheData(PropertyBag);
                // DAL.PropertyBags.Update(PropertyBag);
            }
            //   _propertyBagService.UpdateCacheData(ShoppingCart);
            var items = new List<Item>();
            if (ShoppingCart.Items.Where(i => i.Type == itemType).Count() > 0)
            {
                var itemCodes = ShoppingCart.Items.Where(i => i.Type == itemType).Select(c => c.ItemCode);
                var languageID = Language.GetSelectedLanguageID();
                items = _propertyBagItem.GetItems(ShoppingCart.Items.Where(i => i.Type == itemType), OrderConfiguration, languageID, (itemType == ShoppingCartItemType.AutoOrder) ? AutoOrderConfiguration.PriceTypeID : OrderConfiguration.PriceTypeID).ToList();
                foreach (var cartItem in items)
                {
                    var shoppingCartItem = ShoppingCart.Items.Where(c => c.ItemCode == cartItem.ItemCode).FirstOrDefault();
                    cartItem.Quantity = shoppingCartItem.Quantity;
                }
                var specialItem = ShoppingCart.Items.FirstOrDefault(x => !string.IsNullOrEmpty(x.Field5));
                if (specialItem != null)
                {
                    var itm = items.FirstOrDefault(x => x.ID == specialItem.ID);
                    if (itm != null)
                    {
                        itm.PriceEachOverride = Decimal.Parse(specialItem.Field5);
                        itm.Price = Decimal.Parse(specialItem.Field5);
                        itm.BusinessVolumeEachOverride = Decimal.Parse(specialItem.Field5);
                        itm.CommissionableVolumeEachOverride = Decimal.Parse(specialItem.Field5);
                        itm.TaxableEachOverride = Decimal.Parse(specialItem.Field5);
                        itm.ShippingPriceEachOverride = itm.OtherCheck1 == true ? 0M : Decimal.Parse(specialItem.Field5);
                    }
                }
                subtotal = items.Sum(c => c.Quantity * c.Price);
            }
            var cookie = Request.Cookies[_config.Value.Globalization.CookieKey + "UserID"]; var dripItems = new List<Items>();
            foreach (var itm in items)
            {
                dripItems.Add(new Items
                {
                    product_id = itm.ItemID.ToString(),
                    name = itm.ItemDescription,
                    price = itm.Price,
                    quantity = itm.Quantity,
                    sku = itm.ItemCode,
                    image_url = new Uri(_config.Value.Company.BaseReplicatedUrl + itm.LargeImageUrl).AbsoluteUri
                });
            }
            var dripMail = new DripMail();
            dripMail.Enqueue(new CartDripData
            {
                Type = 1,
                provider = "Wink Exigo",
                // need to implement with cookies
                // person_id = cookie != null ? cookie.Value : ShoppingCart.SessionID,
                person_id = 1,
                // need to implement with new implementation
                // email = Identity.Customer?.Email,
                email = "testc@gmail.com",
                action = dripItems.Count() > 1 ? "updated" : "created",
                cart_id = ShoppingCart.SessionID,
                cart_url = "https://winknaturals.com/WinkCorporate/store/cart",
                items = dripItems
            });
            return Ok();
        }

        [HttpPost("UpdateItemQuantity")]
        public IActionResult UpdateItemQuantity(Guid id, decimal quantity)
        {
            var item = ShoppingCart.Items.Where(c => c.ID == id).FirstOrDefault();
            var cartType = item.Type; ShoppingCart.Items.Update(id, quantity);
            // _propertyBagService.UpdateCacheData(ShoppingCart); 
            var subtotal = 0M;
            if (quantity == 0)
            {
                item.Quantity = 0;
            }
            var items = new List<Item>();
            if (ShoppingCart.Items.Count() > 0)
            {
                var itemCodes = ShoppingCart.Items.Where(c => c.Type == cartType).Select(c => c.ItemCode);
                var languageID = Language.GetSelectedLanguageID();

                foreach (var cartItem in items)
                {
                    var shoppingCartItem = ShoppingCart.Items.Where(c => c.ItemCode == cartItem.ItemCode && c.Type == cartType).FirstOrDefault();
                    cartItem.Quantity = shoppingCartItem.Quantity;
                }



                var specialItem = ShoppingCart.Items.FirstOrDefault(x => !string.IsNullOrEmpty(x.Field5));
                if (specialItem != null)
                {
                    var itm = items.FirstOrDefault(x => x.ID == specialItem.ID);
                    if (itm != null)
                    {
                        itm.PriceEachOverride = Decimal.Parse(specialItem.Field5);
                        itm.Price = Decimal.Parse(specialItem.Field5);
                        itm.BusinessVolumeEachOverride = Decimal.Parse(specialItem.Field5);
                        itm.CommissionableVolumeEachOverride = Decimal.Parse(specialItem.Field5);
                        itm.TaxableEachOverride = Decimal.Parse(specialItem.Field5);
                        itm.ShippingPriceEachOverride = itm.OtherCheck1 == true ? 0M : Decimal.Parse(specialItem.Field5);
                    }
                }
                subtotal = items.Sum(c => c.Quantity * c.Price);
            }
            var cookie = Request.Cookies[_config.Value.Globalization.CookieKey + "UserID"]; var dripItems = new List<Items>();
            foreach (var itm in items)
            {
                dripItems.Add(new Items
                {
                    product_id = itm.ItemID.ToString(),
                    name = itm.ItemDescription,
                    price = itm.Price,
                    quantity = itm.Quantity,
                    sku = itm.ItemCode,
                    image_url = new Uri(_config.Value.Company.BaseReplicatedUrl + itm.LargeImageUrl).AbsoluteUri
                });
            }
            var dripMail = new DripMail();
            dripMail.Enqueue(new CartDripData
            {
                Type = 1,
                provider = "Wink Exigo",
                // need to implement with cookies
                // person_id = cookie != null ? cookie.Value : ShoppingCart.SessionID,
                person_id = 1,
                // need to implement with new implementation
                // email = Identity.Customer?.Email,
                email = "testc@gmail.com",
                action = dripItems.Count() > 1 ? "updated" : "created",
                cart_id = ShoppingCart.SessionID,
                cart_url = "https://winknaturals.com/WinkCorporate/store/cart",
                items = dripItems
            });
            return Ok();
        }

        [HttpPost("SetShipMethodID")]
        public IActionResult SetShipMethodID(int shipMethodID)
        {
            PropertyBag.ShipMethodID = shipMethodID;
            // If Auto Order, update the Auto Order ship method id too
            if (ShoppingCart.Items.Any(i => i.Type == ShoppingCartItemType.AutoOrder))
            {
                PropertyBag.AutoOrderShipMethodID = shipMethodID;
            }
            //  _propertyBagService.UpdateCacheData(PropertyBag);
            // DAL.PropertyBags.Update(PropertyBag);
            return Ok();
        }

        #region Private methods

        private void PayUsingPointAccount(int newOrderID, decimal pointPaymentAmount, CustomerPointAccount pointAccount)
        {
            try
            {
                var pointPaymentRequest = new CreatePaymentPointAccountRequest()
                {
                    OrderID = newOrderID,
                    PointAccountID = pointAccount.PointAccountID,
                    PaymentDate = DateTime.Now,
                    Amount = pointPaymentAmount
                };

                var pointPaymentResponse = _exigoApiContext.GetContext(true).CreatePaymentPointAccountAsync(pointPaymentRequest);//_exigoApiContext.GetContext().CreatePaymentPointAccount(pointPaymentRequest);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Resources.CommonResource.PointPaymentError4, ex.Message));
            }
        }

        private string GetFormattedUrl(string webAlias)
        {
            var BaseReplicatedUrl = "https://winknaturals.com" + "/{0}";
            return BaseReplicatedUrl.FormatWith(webAlias);

        }

        #endregion

        [HttpGet("Testing")]
        public IActionResult Testing()
        {
            return Ok(Identity);
        }

       // [Route("thank-you")]
        [HttpGet("thank-you")]
        public IActionResult OrderComplete(int orderId = 0, decimal orderTotal = 0)
        {
            _propertyBagService.Delete(ShoppingCart);
            _propertyBagService.Delete(PropertyBag);
            return Ok();
        }

        [HttpPost("AutoOrder")]
        public ActionResult AutoOrder()
        {
            var model = Models.ShoppingViewModelFactory.Create<AutoOrderSettingsViewModel>(PropertyBag);
            // Ensure we have a valid frequency type
            if (!Autoorder.AvailableFrequencyTypes.Contains(PropertyBag.AutoOrderFrequencyType))
            {
                PropertyBag.AutoOrderFrequencyType = Autoorder.AvailableFrequencyTypes.FirstOrDefault();
            }

            // Ensure we have a valid start date based on the frequency
            if (PropertyBag.AutoOrderStartDate == DateTime.MinValue.ToCST())
            {
                PropertyBag.AutoOrderStartDate = DateTime.Now.ToCST();
            }

            // Set our model
            model.AutoOrderStartDate = PropertyBag.AutoOrderStartDate;
            model.AutoOrderFrequencyType = PropertyBag.AutoOrderFrequencyType;
            return Ok(model);
        }

        // To Set Item CreateParty
        [HttpPost("CreateParty")]
        public IActionResult CreateParty(CreatePartyRequest createPartyRequest)
        {
            return Ok(_shoppingService.CreateParty(createPartyRequest));
        }

        [HttpGet("GetPartyAccountId")]
        public IActionResult GetPartyAccountId(int partyId)
        {
            return Ok(_shoppingService.GetParty(partyId));
        }

        [HttpGet("GetReviewOrder")]
        public IActionResult GetReviewOrder(int customerId,int OrderId)
        {
            return Ok(_shoppingService.GetCustomerReviewOrder(Identity.CustomerID, OrderId));
        }

      

        //[HttpGet("Review")]
        //public  async  Task<IActionResult> Review()
        //{
        //    //var model = ShoppingViewModelFactory.Create<OrderReviewViewModel>(PropertyBag);
        //    //model.Coupon = PropertyBag.Coupon;

        //    //var languageID = Language.GetSelectedLanguageID();
        //    //var cartItems = new List<Item>();
        //    //// Get the cart items

        //    //var cartOrderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.Order);
        //    //var hasOrder = cartOrderItems.Count() > 0;
        //    //var cartAutoOrderItems = ShoppingCart.Items.Where(c => c.Type == ShoppingCartItemType.AutoOrder);
        //    //var hasAutoOrder = cartAutoOrderItems.Count() > 0;

        //    //if (hasOrder)
        //    //{
        //    //    cartItems = _propertyBagItem.GetItems(cartOrderItems, OrderConfiguration, languageID).ToList();
        //    //}

        //    //if (hasAutoOrder)
        //    //{
        //    //    cartItems.AddRange(_propertyBagItem.GetItems(cartAutoOrderItems, OrderConfiguration,languageID).ToList());
        //    //}
        //    //model.Items = cartItems;
        //    //var specialItem = ShoppingCart.Items.FirstOrDefault(x => x.Field5 != null && x.Field5 != string.Empty);
        //    //if (specialItem != null && !string.IsNullOrEmpty(specialItem.Field5))
        //    //{
        //    //    var itm = cartItems.FirstOrDefault(x => x.ID == specialItem.ID);
        //    //    if (itm != null)
        //    //    {
        //    //        itm.PriceEachOverride = Decimal.Parse(specialItem.Field5);
        //    //        itm.Price = Decimal.Parse(specialItem.Field5);
        //    //        itm.BusinessVolumeEachOverride = Decimal.Parse(specialItem.Field5);
        //    //        itm.CommissionableVolumeEachOverride = Decimal.Parse(specialItem.Field5);
        //    //        itm.TaxableEachOverride = Decimal.Parse(specialItem.Field5);
        //    //        itm.ShippingPriceEachOverride = itm.OtherCheck1 == true ? 0M : Decimal.Parse(specialItem.Field5);
        //    //    }
        //    //}

        //    //// Calculate the order if applicable
        //    //var orderItems = cartItems.Where(c => c.Type == ShoppingCartItemType.Order).ToList();
        //    //if (orderItems.Count > 0)
        //    //{
        //    //    #region Order Totals
        //    //    var beginningShipMethodID = PropertyBag.ShipMethodID;
        //    //    // If this is the first time coming to the page, and the property bag's ship method has not been set, then set it to the default for the configuration
        //    //    if (PropertyBag.ShipMethodID == 0)
        //    //    {
        //    //        PropertyBag.ShipMethodID = OrderConfiguration.DefaultShipMethodID;
        //    //        beginningShipMethodID = PropertyBag.ShipMethodID;
        //    //        _propertyBagService.UpdateCacheData(PropertyBag);
        //    //    }
        //    //    var request = new OrderCalculationRequest()
        //    //    {
        //    //        Configuration = OrderConfiguration,
        //    //         Items = orderItems,
        //    //        Address = (IAddress)PropertyBag.ShippingAddress,
        //    //        ShipMethodID = PropertyBag.ShipMethodID,
        //    //        CustomerID = Identity.CustomerID,
        //    //        Other17 = PropertyBag.QuantityOfPointsToUse.ToString() // Points
        //    //    };
        //    //    if (PropertyBag.Coupon != null && String.IsNullOrEmpty(PropertyBag.Coupon.Code))
        //    //    {
        //    //        request.Other16 = PropertyBag.Coupon.Code;
        //    //    }
        //    //    if (PropertyBag.ContainsSpecial)
        //    //    {
        //    //        request.Other18 = "true";
        //    //    }
        //    //    model.OrderTotals = (OrderCalculationResponse)_shoppingService.CalculateOrder(request);

        //    //    if (model.OrderTotals.Details.Any(d => d.ItemCode.ToUpper() == "COUPON"))
        //    //    {
        //    //        var couponItem = model.OrderTotals.Details.FirstOrDefault(i => i.ItemCode.ToUpper() == "COUPON");

        //    //        model.Coupon.CouponCode = couponItem.ItemCode;
        //    //        model.Coupon.CouponQuantity = couponItem.Quantity;
        //    //        model.Coupon.CouponPriceEach = couponItem.PriceEach;
        //    //        model.Coupon.CouponItemDescription = couponItem.ItemDescription;
        //    //    }

        //    //    model.ShipMethods = (IEnumerable<WinkNatural.Web.Services.DTO.Shopping.CalculateOrder.IShipMethod>)model.OrderTotals.ShipMethods;
        //    //    if (PropertyBag.ShippingAddress.State != "UT")
        //    //    {
        //    //        model.ShipMethods = model.ShipMethods.Where(v => v.ShipMethodID != 9);
        //    //    }

        //    //    // Set the default ship method
        //    //    if (model.ShipMethods.Count() > 0)
        //    //    {
        //    //        if (model.ShipMethods.Any(c => c.ShipMethodID == PropertyBag.ShipMethodID))
        //    //        {
        //    //            // If the property bag ship method ID exists in the results from order calc, set the correct result as selected                
        //    //            model.ShipMethods.First(c => c.ShipMethodID == PropertyBag.ShipMethodID).Selected = true;
        //    //        }
        //    //        else
        //    //        {
        //    //            // If we don't have the ship method we're supposed to select, check the first one, save the selection and recalculate
        //    //            model.ShipMethods.First().Selected = true;

        //    //            // If for some reason the property bag is outdated and the ship method stored in it is not in the list, set the first result as selected and re-set the property bag's value
        //    //            PropertyBag.ShipMethodID = model.ShipMethods.FirstOrDefault().ShipMethodID;
        //    //            _propertyBagService.UpdateCacheData(PropertyBag);
        //    //        }
        //    //    }

        //    //    // If the original property bag value has changed from the beginning of the call, re-calculate the values
        //    //    if (beginningShipMethodID != PropertyBag.ShipMethodID)
        //    //    {
        //    //        request = new OrderCalculationRequest()
        //    //        {
        //    //            Configuration = OrderConfiguration,
        //    //            Items = orderItems,
        //    //            Address = PropertyBag.ShippingAddress,
        //    //            ShipMethodID = PropertyBag.ShipMethodID,
        //    //            ReturnShipMethods = false,
        //    //            CustomerID = Identity.CustomerID,
        //    //            Other17 = PropertyBag.QuantityOfPointsToUse.ToString() // Points
        //    //        };

        //    //        if (PropertyBag.Coupon != null && String.IsNullOrEmpty(PropertyBag.Coupon.Code))
        //    //        {
        //    //            request.Other16 = PropertyBag.Coupon.Code;
        //    //        }
        //    //        if (PropertyBag.ContainsSpecial)
        //    //        {
        //    //            request.Other18 = "true";
        //    //        }

        //    //        var newCalculationResult =  _shoppingService.CalculateOrder(request); 

        //    //        if (model.OrderTotals.Details.Any(d => d.ItemCode == "Coupon"))
        //    //        {
        //    //            var couponItem = model.OrderTotals.Details.FirstOrDefault(i => i.ItemCode == "Coupon");

        //    //            model.Coupon.CouponCode = couponItem.ItemCode;
        //    //            model.Coupon.CouponQuantity = couponItem.Quantity;
        //    //            model.Coupon.CouponPriceEach = couponItem.PriceEach;
        //    //            model.Coupon.CouponItemDescription = couponItem.ItemDescription;
        //    //        }

        //    //        model.OrderTotals = (OrderCalculationResponse)newCalculationResult;
        //    //    }
        //    //    #endregion
        //    //}

        //    //// Calculate the autoorder if applicable
        //    //var autoOrderItems = cartItems.Where(c => c.Type == ShoppingCartItemType.AutoOrder).ToList();

        //    //// Keep prices as they were when creating autoorder
        //    //foreach (var itm in autoOrderItems)
        //    //{
        //    //    itm.PriceEachOverride = itm.Price;
        //    //    itm.BusinessVolumeEachOverride = itm.Price;
        //    //    itm.CommissionableVolumeEachOverride = itm.Price;
        //    //    itm.TaxableEachOverride = itm.Price;
        //    //    itm.ShippingPriceEachOverride = itm.OtherCheck1 == true ? 0 : itm.Price;
        //    //}

        //    //if (autoOrderItems.Count > 0)
        //    //{
        //    //    #region Auto Order Totals

        //    //    var defaultAutoOrderShipMethodID = 8; //Autoorder.DefaultAutoOrderShipMethodID;

        //    //    model.AutoOrderTotals = (OrderCalculationResponse)_shoppingService.CalculateOrder(new OrderCalculationRequest
        //    //    {
        //    //        Configuration = AutoOrderConfiguration,
        //    //        Items = autoOrderItems,
        //    //        Address = PropertyBag.ShippingAddress,
        //    //        ShipMethodID = defaultAutoOrderShipMethodID,
        //    //        ReturnShipMethods = true,
        //    //        OrderTypeID = OrderTypes.RecurringOrder
        //    //        // Other17 = PropertyBag.QuantityOfPointsToUse.ToString() // Points
        //    //    });
        //    //    model.AutoOrderShipMethods = (IEnumerable<WinkNatural.Web.Services.DTO.Shopping.CalculateOrder.IShipMethod>)model.AutoOrderTotals.ShipMethods;

        //    //    // Set the default ship method
        //    //    if (model.AutoOrderShipMethods.Count() > 0)
        //    //    {
        //    //        if (model.AutoOrderShipMethods.Any(c => c.ShipMethodID == PropertyBag.AutoOrderShipMethodID))
        //    //        {
        //    //            // If the property bag ship method ID exists in the results from order calc, set the correct result as selected                
        //    //            model.AutoOrderShipMethods.First(c => c.ShipMethodID == PropertyBag.AutoOrderShipMethodID).Selected = true;
        //    //        }
        //    //        else
        //    //        {
        //    //            // If we don't have the ship method we're supposed to select, check the first one, save the selection and recalculate
        //    //            model.AutoOrderShipMethods.First().Selected = true;

        //    //            // If for some reason the property bag is outdated and the ship method stored in it is not in the list, set the first result as selected and re-set the property bag's value
        //    //            PropertyBag.AutoOrderShipMethodID = model.AutoOrderShipMethods.FirstOrDefault().ShipMethodID;
        //    //            _propertyBagService.UpdateCacheData(PropertyBag);
        //    //        }
        //    //    }

        //    //    // If the original property bag value has changed from the beginning of the call, re-calculate the values
        //    //    if (defaultAutoOrderShipMethodID != 0 && defaultAutoOrderShipMethodID != PropertyBag.AutoOrderShipMethodID)
        //    //    {
        //    //        var newCalculationResult = _shoppingService.CalculateOrder(new OrderCalculationRequest
        //    //        {
        //    //            Configuration = AutoOrderConfiguration,
        //    //            Items = autoOrderItems,
        //    //            Address = PropertyBag.ShippingAddress,
        //    //            ShipMethodID = PropertyBag.AutoOrderShipMethodID,
        //    //            ReturnShipMethods = true,
        //    //            CustomerID = Identity.CustomerID,
        //    //            Other17 = PropertyBag.QuantityOfPointsToUse.ToString() // Points
        //    //        });

        //    //        model.AutoOrderTotals = (OrderCalculationResponse)newCalculationResult;
        //    //        model.AutoOrderShipMethods = (IEnumerable<WinkNatural.Web.Services.DTO.Shopping.CalculateOrder.IShipMethod>)newCalculationResult;
        //    //    }

        //    //    if (orderItems.Count == 0)
        //    //    {
        //    //        model.ShipMethods = (IEnumerable<WinkNatural.Web.Services.DTO.Shopping.CalculateOrder.IShipMethod>)model.AutoOrderTotals.ShipMethods;

        //    //        if (PropertyBag.ShipMethodID != PropertyBag.AutoOrderShipMethodID)
        //    //        {
        //    //            PropertyBag.ShipMethodID = PropertyBag.AutoOrderShipMethodID;
        //    //            _propertyBagService.UpdateCacheData(PropertyBag);
        //    //        }
        //    //    }
        //    //    #endregion
        //    //}

        //    //if (PropertyBag.UsePointsAsPayment)
        //    //{
        //    //    model.LoyaltyPointAccount = (CustomerPointAccount)_customerPointAccount.GetCustomerPointAccounts(Identity.CustomerID, 1);

        //    //    if (model.LoyaltyPointAccount != null && model.LoyaltyPointAccount.Balance > 0)
        //    //    {
        //    //        model.HasValidPointAccount = true;
        //    //        // Now we want to do a final check to ensure that the customer can actually has enough points, and if not we need to make sure they have previously entered a Payment Method. 
        //    //        // If the user has not entered a payment method and their point balance is not enough to cover the Total of the Order, we need to inform the user they must go back to the Payment page to add one.
        //    //        if (model.LoyaltyPointAccount.Balance < model.OrderTotals.Subtotal && PropertyBag.PaymentMethod == null)
        //    //        {
        //    //            //ViewBag.ErrorMessage = Resources.Common.PointPaymentError2;
        //    //        }
        //    //    }

        //    //    decimal nonPointableSubTotal = 0;
        //    //    foreach (var item in model.Items)
        //    //    {
        //    //        if (item.OtherCheck2 || item.Field5 != "")
        //    //        {
        //    //            nonPointableSubTotal += item.Price * item.Quantity;
        //    //        }
        //    //    }
        //    //    var pointableTotal = model.OrderTotals.Subtotal - nonPointableSubTotal;
        //    //    if (nonPointableSubTotal == model.OrderTotals.Subtotal)
        //    //    {
        //    //        PropertyBag.QuantityOfPointsToUse = 0;
        //    //    }
        //    //    else if (PropertyBag.QuantityOfPointsToUse > pointableTotal / 2)
        //    //    {
        //    //        PropertyBag.QuantityOfPointsToUse = pointableTotal / 2;
        //    //    }
        //    //    model.QuantityOfPointsToUse = PropertyBag.QuantityOfPointsToUse;
        //    //}

        //    //return Ok(model);

        //}
        [HttpGet("GetCustomer")]
        public IActionResult GetCustomer(int partyId)
        {
            return Ok(_shoppingService.GetCustomer(partyId));
        }
       
    }
}
