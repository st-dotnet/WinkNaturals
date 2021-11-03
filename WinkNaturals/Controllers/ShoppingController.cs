﻿using Microsoft.AspNetCore.Mvc;
using System;
using WinkNatural.Web.Services.Interfaces;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Models;
using Exigo.Api.Client;
using AutoMapper;
using WinkNaturals.Setting;
using Microsoft.Extensions.Options;
using WinkNaturals.Models.Shopping.Interfaces;
using System.Linq;
using WinkNaturals.WebDrip;
using System.Collections.Generic;
using WinkNaturals.Utilities.WebDrip;
using WinkNaturals.Setting.Interfaces;
using WinkNaturals.Models.Shopping.Checkout;

namespace WinkNaturals.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        private readonly IShoppingService _shoppingService;
        private readonly IPropertyBags _propertyBagService;
        private readonly IPropertyBagItem _propertyBagItem;
        private readonly IOrderConfiguration _orderConfiguration;
        private readonly IMapper _mapper;
        private readonly IOptions<ConfigSettings> _config;
        private readonly ISqlCacheService _sqlCacheService;
        private readonly IGetCurrentMarket _getCurrentMarket;


        public IOrderConfiguration OrderConfiguration { get; set; }
        public ShoppingController(IShoppingService shoppingService, IMapper mapper, IOptions<ConfigSettings> config, ISqlCacheService sqlCacheService, IPropertyBags propertyBagService, IPropertyBagItem propertyBagItem, IOrderConfiguration orderConfiguration, IGetCurrentMarket getCurrentMarket)
        {
            _shoppingService = shoppingService;
            _mapper = mapper;
            _config = config;
            _sqlCacheService = sqlCacheService;
            _propertyBagService = propertyBagService;
            _propertyBagItem = propertyBagItem;
            _orderConfiguration = orderConfiguration;
            _getCurrentMarket = getCurrentMarket;

        }

        public ShoppingCartItemsPropertyBag ShoppingCart
        {
            get
            {
                if (_shoppingCart == null)
                {
                    _shoppingCart = _propertyBagService.GetCacheData<ShoppingCartItemsPropertyBag>(_config.Value.Globalization.CookieKey + "ReplicatedSiteShopping" + "ShoppingCart");
                }
                return _shoppingCart;
            }
        }

       // working....
        public ShoppingCartCheckoutPropertyBag PropertyBag
        {
            get
            {
                if (_propertyBag == null)
                {
                    _propertyBag = null;//DAL.PropertyBags.Get<ShoppingCartCheckoutPropertyBag>(_config.Value.Globalization.CookieKey + "ReplicatedSiteShopping" + "PropertyBag");
                }
                return _propertyBag;
            }
        }
        private ShoppingCartCheckoutPropertyBag _propertyBag;
        private ShoppingCartItemsPropertyBag _shoppingCart;
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
        [HttpPost("AddToCart")]
        public IActionResult AddToCart(Item item)
        {
            var CountryCode = "US";
            OrderConfiguration = _getCurrentMarket.curretMarket(CountryCode).GetConfiguration().Orders;
            
            bool checkQuantity = ShoppingCart.Items.CheckItemBackOffice(item);
            if (!checkQuantity)
            {
                return Ok();
            }
            ShoppingCart.Items.Add(item);
            _propertyBagService.UpdateCacheData(ShoppingCart);

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
                // need to implement with cookies
                //  person_id = cookie != null ? cookie.Value : ShoppingCart.SessionID,
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

        /// <summary>
        /// SubmitCheckout
        /// </summary>
        /// <returns></returns>
        [HttpPost("SubmitCheckout")]
        public IActionResult SubmitCheckout(TransactionalRequestModel transactionRequests)
        {
            return Ok(_shoppingService.SubmitCheckout(transactionRequests));
        }
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

        [HttpGet("GetOrder")]
        public IActionResult GetOrder(GetOrdersRequest ordersRequest)
        {
            return Ok(_shoppingService.GetOrder(ordersRequest));
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
            return Ok(_shoppingService.GetPromoDetail(promoCode));
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
                ShoppingCart.Items.Add(item);
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
               //  PropertyBags.Update(PropertyBag);
            }
            ShoppingCart.Items.Remove(id); if (ShoppingCart.Items.Count() == 1 && !string.IsNullOrEmpty(ShoppingCart.Items.First().Field5))
            {
                ShoppingCart.Items.Remove(ShoppingCart.Items.First().ID);
                // PropertyBag.ContainsSpecial = false;
                // DAL.PropertyBags.Update(PropertyBag);
            }
            _propertyBagService.UpdateCacheData(ShoppingCart); var items = new List<Item>();
            if (ShoppingCart.Items.Where(i => i.Type == itemType).Count() > 0)
            {
                var itemCodes = ShoppingCart.Items.Where(i => i.Type == itemType).Select(c => c.ItemCode);
                var languageID = Language.GetSelectedLanguageID();
                //items = _propertyBagItem.GetItems(ShoppingCart.Items.Where(i => i.Type == itemType), OrderConfiguration, languageID, (itemType == ShoppingCartItemType.AutoOrder) ? AutoOrderConfiguration.PriceTypeID : OrderConfiguration.PriceTypeID).ToList();
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
            _propertyBagService.UpdateCacheData(ShoppingCart); var subtotal = 0M; if (quantity == 0)
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
            //PropertyBag.ShipMethodID = shipMethodID;
            // If Auto Order, update the Auto Order ship method id too
            if (ShoppingCart.Items.Any(i => i.Type == ShoppingCartItemType.AutoOrder))
            {
                // PropertyBag.AutoOrderShipMethodID = shipMethodID;
            }
            // DAL.PropertyBags.Update(PropertyBag);
            return Ok();
        }
    }
}
