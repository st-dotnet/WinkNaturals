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
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Infrastructure.Services.ExigoService.CreditCard;
using static WinkNaturals.Helpers.Constant;
using Microsoft.AspNetCore.Authorization;

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
        private readonly ICustomerAutoOreder _customerAutoService;
        private readonly IEnrollmentService _enrollmentService;

        public IOrderConfiguration OrderConfiguration { get; set; }
        public IOrderConfiguration AutoOrderConfiguration { get; set; }
        public ShoppingController(IShoppingService shoppingService, IMapper mapper, IOptions<ConfigSettings> config, ISqlCacheService sqlCacheService, IPropertyBags propertyBagService, IPropertyBagItem propertyBagItem, IOrderConfiguration orderConfiguration, 
            IGetCurrentMarket getCurrentMarket, IConfiguration configuration, ICustomerPointAccount customerPointAccount,IAutoOrders autoOrders, IDistributedCache distributedCache, IExigoApiContext exigoApiContext, ICustomerAutoOreder customerAutoService,IEnrollmentService enrollmentService)
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
            _customerAutoService = customerAutoService;
            _enrollmentService = enrollmentService;
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
        public async Task<IActionResult> GetProductList(int categoryID, int sortBy)
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
        public async Task<IActionResult> SubmitCheckout(TransactionalRequestModel transactionRequests)
        {          
            return Ok(await _shoppingService.SubmitCheckout(transactionRequests, Identity.CustomerID, Identity.Email));
        }


        /// <summary>
        /// CalculateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("CalculateOrder")]
        public async Task<IActionResult> CalculateOrder(CalculateOrderRequest calculateOrderReq, int shipMethodID = 0)
        {
            return Ok(await _shoppingService.CalculateOrder(calculateOrderReq));
        }

        /// <summary>
        /// CreateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest createOrderRequest)
        {
            return Ok(await _shoppingService.CreateOrder(createOrderRequest));
        }

        /// <summary>
        /// CreateOrderImport
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateOrderImport")]
        public async Task<IActionResult> CreateOrderImport(CreateOrderImportRequest createOrderImportRequest)
        {
            return Ok(await _shoppingService.CreateOrderImport(createOrderImportRequest));
        }

        /// <summary>
        /// UpdateOrder
        /// </summary>
        /// <returns></returns>
        [HttpPost("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(UpdateOrderRequest updateOrderRequest)
        {
            return Ok(await _shoppingService.UpdateOrder(updateOrderRequest));
        }

        /// <summary>
        /// ChangeOrderStatus
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangeOrderStatus")]
        public async Task<IActionResult> ChangeOrderStatus(ChangeOrderStatusRequest changeOrderStatusRequest)
        {
            return Ok(await _shoppingService.ChangeOrderStatus(changeOrderStatusRequest));
        }

        /// <summary>
        /// ChangeOrderStatusBatch
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangeOrderStatusBatch")]
        public async Task<IActionResult> ChangeOrderStatusBatch(ChangeOrderStatusBatchRequest changeOrderStatusBatchRequest)
        {
            return Ok(await _shoppingService.ChangeOrderStatusBatch(changeOrderStatusBatchRequest));
        }

        /// <summary>
        /// ValidateCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ValidateCreditCardToken")]
        public async Task<IActionResult> ValidateCreditCardToken(ValidateCreditCardTokenRequest creditCardTokenRequest)
        {
            return Ok(await _shoppingService.ValidateCreditCardToken(creditCardTokenRequest));
        }

        /// <summary>
        /// CreatePayment
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePayment")]
        public async Task<IActionResult> CreatePayment(CreatePaymentRequest createPaymentRequest)
        {
            return Ok(await _shoppingService.CreatePayment(createPaymentRequest));
        }

        /// <summary>
        /// CreatePaymentCreditCard
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentCreditCard")]
        public async Task<IActionResult> CreatePaymentCreditCard(CreatePaymentCreditCardRequest createPaymentCreditCardRequest)
        {
            return Ok(await _shoppingService.CreatePaymentCreditCard(createPaymentCreditCardRequest));
        }

        /// <summary>
        /// CreatePaymentWallet
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentWallet")]
        public async Task<IActionResult> CreatePaymentWallet(CreatePaymentWalletRequest createPaymentWalletRequest)
        {
            return Ok(await _shoppingService.CreatePaymentWallet(createPaymentWalletRequest));
        }

        /// <summary>
        /// CreatePaymentPointAccount
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentPointAccount")]
        public async Task<IActionResult> CreatePaymentPointAccount(CreatePaymentPointAccountRequest createPaymentPointAccountRequest)
        {
            return Ok( await _shoppingService.CreatePaymentPointAccount(createPaymentPointAccountRequest));
        }

        /// <summary>
        /// CreatePaymentCheck
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreatePaymentCheck")]
        public async Task<IActionResult> CreatePaymentCheck(CreatePaymentCheckRequest createPaymentCheckRequest)
        {
            return Ok( await _shoppingService.CreatePaymentCheck(createPaymentCheckRequest));
        }

        /// <summary>
        /// ChargeCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeCreditCardToken")]
        public async Task<IActionResult> ChargeCreditCardToken(ChargeCreditCardTokenRequest chargeCreditCardTokenRequest)
        {
            return Ok(await _shoppingService.ChargeCreditCardToken(chargeCreditCardTokenRequest));
        }

        /// <summary>
        /// ChargeCreditCardTokenOnFile
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeCreditCardTokenOnFile")]
        public async Task<IActionResult> ChargeCreditCardTokenOnFile(ChargeCreditCardTokenOnFileRequest chargeCreditCardTokenOnFileRequest)
        {
            return Ok(await _shoppingService.ChargeCreditCardTokenOnFile(chargeCreditCardTokenOnFileRequest));
        }

        /// <summary>
        /// ChargeGroupOrderCreditCardToken
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChargeGroupOrderCreditCardToken")]
        public async Task<IActionResult> ChargeGroupOrderCreditCardToken(ChargeGroupOrderCreditCardTokenRequest chargeGroupOrderCredit)
        {
            return Ok(await _shoppingService.ChargeGroupOrderCreditCardToken(chargeGroupOrderCredit));
        }

        /// <summary>
        /// RefundPriorCreditCardCharge
        /// </summary>
        /// <returns></returns>
        [HttpPost("RefundPriorCreditCardCharge")]
        public async Task<IActionResult> RefundPriorCreditCardCharge(RefundPriorCreditCardChargeRequest refundPriorCredit)
        {
            return Ok(await _shoppingService.RefundPriorCreditCardCharge(refundPriorCredit));
        }
        /// <summary>
        /// RefundPriorCreditCardCharge
        /// </summary>F
        /// <returns></returns>
        [HttpPost]
        [Route("checkout/shipping")]
        public async Task<IActionResult> Shipping(VerifyAddressRequest addressRequest)
        {
            return Ok(await _shoppingService.Shipping(addressRequest));
        }
        [HttpGet]
        [Route("GetshippingAddress/{CustomerID:int}")]
        public IActionResult GetshippingAddress(int CustomerID)
        {
            return Ok(_shoppingService.GetCustomerAddress(CustomerID));
        }

        //[HttpPost("AddUpdateCustomerAddress")]
        //public IActionResult AddUpdateCustomerAddress(ShippingAddress address)
        //{
        //    return Ok(_shoppingService.AddUpdateCustomerAddress(Identity.CustomerID, address));
        //}

        [HttpPost("GetWarehouses")]
        public async Task<IActionResult> GetWarehouses(GetWarehousesRequest warehousesRequest)
        {
            return Ok(await _shoppingService.GetWarehouses(warehousesRequest));
        }
        [HttpGet("SearchProducts/{query}")]
        public IActionResult SearchProducts(string query)
        {
            return Ok(_shoppingService.SearchProducts(query));
        }

        // To implement Special Item block in Cart
        [HttpGet("GetSpecialItem")]
        public IActionResult GetSpecialItem()
        {
            return Ok(_shoppingService.GetSpecialItem());
        }

        [HttpGet("GetCustomerRealTime")]
        //To Get customer detail for editing.
        public async Task<IActionResult> GetCustomerRealTime()
        {
            return Ok(await _shoppingService.GetCustomerRealTime(Identity.CustomerID));
        }

        //To update customer detail
        [HttpPost("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(UpdateCustomerRequest updateCustomerRequest)
        {
            return Ok(await  _shoppingService.UpdateCustomer(updateCustomerRequest));
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
        public async Task<IActionResult> CreateItem(CreateItemRequest createItemRequest)
        {
            return Ok(await _shoppingService.CreateItem(createItemRequest));
        }

        /// <summary>
        /// To Update Item
        /// </summary>
        /// <returns></returns>

        [HttpPost("UpdateItem")]
        public async Task<IActionResult> UpdateItem(UpdateItemRequest updateItemRequest)
        {
            return Ok(await _shoppingService.UpdateItem(updateItemRequest));
        }

        /// <summary>
        /// To Set Item Price
        /// </summary>
        /// <returns></returns>

        [HttpPost("SetItemPrice")]
        public async Task<IActionResult> SetItemPrice(SetItemPriceRequest setItemPriceRequest)
        {
            return Ok(await _shoppingService.SetItemPrice(setItemPriceRequest));
        }

        /// <summary>
        /// To Set Item Warehouse
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetItemWarehouse")]
        public async Task<IActionResult> SetItemWarehouse(SetItemWarehouseRequest setItemWarehouseRequest)
        {
            return Ok(await _shoppingService.SetItemWarehouse(setItemWarehouseRequest));
        }

        /// <summary>
        /// To Set Item CountryRegion
        /// </summary>
        /// <returns></returns>
        [HttpPost("SetItemCountryRegion")]
        public async Task<IActionResult> SetItemCountryRegion(SetItemCountryRegionRequest setItemWarehouseRequest)
        {
            return Ok(await _shoppingService.SetItemCountryRegion(setItemWarehouseRequest));
        }

        /// <summary>
        /// To Create WebCategory
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateWebCategory")]
        public async Task<IActionResult> CreateWebCategory(CreateWebCategoryRequest createWebCategoryRequest)
        {
            return Ok(await _shoppingService.CreateWebCategory(createWebCategoryRequest));
        }

        /// <summary>
        /// To Update WebCategory
        /// </summary>
        /// <returns></returns>
        [HttpPost("UpdateWebCategory")]
        public async Task<IActionResult> UpdateWebCategory(UpdateWebCategoryRequest updateWebCategoryRequest)
        {
            return Ok(await _shoppingService.UpdateWebCategory(updateWebCategoryRequest));
        }

        // To Delete WebCategory
        [HttpPost("DeleteWebCategory")]
        public async Task<IActionResult> DeleteWebCategory(DeleteWebCategoryRequest deleteWebCategoryRequest)
        {
            return Ok(await _shoppingService.DeleteWebCategory(deleteWebCategoryRequest));
        }

        // To adjust inventory
        [HttpPost("AdjustInventory")]
        public async Task<IActionResult> AdjustInventory(AdjustInventoryRequest adjustInventoryRequest)
        {
            return Ok(await _shoppingService.AdjustInventory(adjustInventoryRequest));
        }

        // To set Item subscription
        [HttpPost("SetItemSubscription")]
        public async Task<IActionResult> SetItemSubscription(SetItemSubscriptionRequest setItemSubscriptionRequest)
        {
            return Ok(await _shoppingService.SetItemSubscription(setItemSubscriptionRequest));
        }

        // To Set Item Point Account
        [HttpPost("SetItemPointAccount")]
        public async Task<IActionResult> SetItemPointAccount(SetItemPointAccountRequest setItemPointAccountRequest)
        {
            return Ok(await _shoppingService.SetItemPointAccount(setItemPointAccountRequest));
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

                var pointPaymentResponse = _exigoApiContext.GetContext(false).CreatePaymentPointAccountAsync(pointPaymentRequest);//_exigoApiContext.GetContext().CreatePaymentPointAccount(pointPaymentRequest);
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
        public async Task<IActionResult> CreateParty(CreatePartyRequest createPartyRequest)
        {
            return Ok(await _shoppingService.CreateParty(createPartyRequest));
        }

        [HttpGet("GetPartyAccountId")]
        public async Task<IActionResult> GetPartyAccountId(int partyId)
        {
            return Ok(await _shoppingService.GetParty(partyId));
        }

        [HttpGet("GetReviewOrder")]
        public async Task<IActionResult> GetReviewOrder(int OrderId)
        {
            return Ok(await _shoppingService.GetCustomerReviewOrder(Identity.CustomerID, OrderId));
        }
        [HttpGet("GetCustomer")]
        public async Task<IActionResult> GetCustomer(int partyId)
        {
            return Ok(await _shoppingService.GetCustomer(partyId));
        }
        /// <summary>
        /// GetProductDetailById by itemCode
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetItemById/{itemCode}")]///{itemCode:string}
        public IActionResult GetItemById(string itemCode, string itemcode2)
        {
            string[] itemCodes = new string[2];
            itemCodes[0] = itemCode;
            itemCodes[1] = itemcode2;
            return Ok(_shoppingService.GetStaticProductDetailById(itemCodes));
        }
        
        /// <summary>
        /// DeleteCustomerAddress
        /// </summary>
        /// <returns></returns>
        /// 
        [AllowAnonymous]
        [HttpDelete("DeleteCustomer")]
        public async Task<IActionResult> DeleteCustomer(Address address)
        {
            return Ok(await _shoppingService.DeleteCustomerAddress(Identity.CustomerID, address));
        }

        [HttpPost("AddUpdateCustomerAddress/{CustomerID:int}")]
        public async Task<IActionResult> AddUpdateCustomerAddress(int CustomerID, ShippingAddress address)
        {
            return Ok(await _shoppingService.AddUpdateCustomerAddress(CustomerID, address));

        }
       

    }
}
