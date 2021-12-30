using Exigo.Api.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinkNatural.Web.Services.DTO.Shopping;
using WinkNaturals.Infrastructure.Services.DTO;
using WinkNaturals.Models.Shopping.Checkout.Coupon.Interfaces;
using WinkNaturals.Models.Shopping.Interfaces.PointAccount;
using Address = WinkNatural.Web.Services.DTO.Shopping.Address;

namespace WinkNatural.Web.Services.Interfaces
{
    public interface IShoppingService
    {
        //Get shop products list
        //Get shop products list
        List<ShopProductsResponse> GetShopProducts(int categoryID, int sortBy, int pageSize = 18, int pageIndex = 0, string[] sizes = null);
        //Get item category
        List<ItemCategoryResponse> GetItemCategory(int webCategoryID);
        CustomerPointAccount GetCustomerLoyaltyPointAccount(int customerId, int LoyaltyPointAccountId);
        List<ShopProductsResponse> GetStaticProductDetailById(string[] itemCodes);
        ShopProductsResponse GetProductDetailById(string[] itemCodes);
       
        ShopProductsResponse AddToCart(ShopProductsResponse shopProducts);
        byte[] GetProductImage(string imageName);
        Task<TransactionalResponse> SubmitCheckout(TransactionalRequestModel transactionRequest, int customerId, string email);

        Task<CalculateOrderResponse> CalculateOrder(CalculateOrderRequest calculateOrder);
        Task<CreateOrderResponse> CreateOrder(CreateOrderRequest createOrderRequest);
        Task<CreateOrderImportResponse> CreateOrderImport(CreateOrderImportRequest createOrderImportRequest);
        Task<UpdateOrderResponse> UpdateOrder(UpdateOrderRequest updateOrderRequest);
        Task<ChangeOrderStatusResponse> ChangeOrderStatus(ChangeOrderStatusRequest changeOrderStatusRequest);
        Task<ChangeOrderStatusBatchResponse> ChangeOrderStatusBatch(ChangeOrderStatusBatchRequest changeOrderStatusBatchRequest);
        Task<ValidateCreditCardTokenResponse> ValidateCreditCardToken(ValidateCreditCardTokenRequest creditCardTokenRequest);
        Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest createPartyRequest);
        Task<CreatePaymentCreditCardResponse> CreatePaymentCreditCard(CreatePaymentCreditCardRequest createPaymentCreditCardRequest);
        Task<CreatePaymentResponse> CreatePaymentWallet(CreatePaymentWalletRequest createPaymentWalletRequest);
        Task<CreatePaymentPointAccountResponse> CreatePaymentPointAccount(CreatePaymentPointAccountRequest createPaymentPointAccountRequest);
        Task<CreatePaymentCheckResponse> CreatePaymentCheck(CreatePaymentCheckRequest createPaymentCheckRequest);
        Task<ChargeCreditCardResponse> ChargeCreditCardToken(ChargeCreditCardTokenRequest chargeCreditCardTokenRequest);
        Task<ChargeCreditCardResponse> ChargeCreditCardTokenOnFile(ChargeCreditCardTokenOnFileRequest chargeCreditCardTokenOnFileRequest);
        Task<ChargeGroupOrderCreditCardTokenResponse> ChargeGroupOrderCreditCardToken(ChargeGroupOrderCreditCardTokenRequest chargeGroupOrderCredit);
        Task<RefundPriorCreditCardChargeResponse> RefundPriorCreditCardCharge(RefundPriorCreditCardChargeRequest refundPriorCredit);
        Task<VerifyAddressResponse> Shipping(VerifyAddressRequest addressRequest);
        List<Address> GetCustomerAddress(int customerID);
        Task<Address> AddUpdateCustomerAddress(int customerID, Address address);
        Task<GetWarehousesResponse> GetWarehouses(GetWarehousesRequest warehousesRequest);
        Task<GetOrdersResponse> GetOrder(GetOrdersRequest ordersRequest, int customerId);
        List<ShopProductsResponse> SearchProducts(string query);

       IEnumerable<ShopProductsResponse> GetItems(GetItemListRequest request, bool includeItemDescriptions = true);
        PromoCode GetPromoDetail(string promoCode, int customerId);

        //To Get customer detail for editing.
        Task<GetCustomersResponse> GetCustomerRealTime(int customerID);

        Task<UpdateCustomerResponse> UpdateCustomer(UpdateCustomerRequest updateCustomerRequest);

        // To create item
        Task<CreateItemResponse> CreateItem(CreateItemRequest createItemRequest);

        Task<UpdateItemResponse> UpdateItem(UpdateItemRequest updateItemRequest);
        Task<SetItemPriceResponse> SetItemPrice(SetItemPriceRequest setItemPriceRequest);
        Task<SetItemWarehouseResponse> SetItemWarehouse(SetItemWarehouseRequest setItemWarehouseRequest);
        Task<SetItemCountryRegionResponse> SetItemCountryRegion(SetItemCountryRegionRequest setItemCountryRegionRequest);
        Task<CreateWebCategoryResponse> CreateWebCategory(CreateWebCategoryRequest createWebCategoryRequest);
        Task<UpdateWebCategoryResponse> UpdateWebCategory(UpdateWebCategoryRequest updateWebCategoryRequest);
        Task<DeleteWebCategoryResponse> DeleteWebCategory(DeleteWebCategoryRequest deleteWebCategoryRequest);
        Task<AdjustInventoryResponse> AdjustInventory(AdjustInventoryRequest adjustInventoryRequest);
        Task<SetItemSubscriptionResponse> SetItemSubscription(SetItemSubscriptionRequest setItemSubscriptionRequest);
        Task<SetItemPointAccountResponse> SetItemPointAccount(SetItemPointAccountRequest setItemPointAccountRequest);
        Task<CreatePartyResponse> CreateParty(CreatePartyRequest createPartyRequest);
        Task<GetPartiesResponse> GetParty(int partyId);

        Task<GetOrdersResponse> GetCustomerReviewOrder(int customerId, int OrderId);
        Task<GetCustomersResponse> GetCustomer(int customerID);
        ShopProductsResponse GetSpecialItem();
        
        Task<UpdateCustomerResponse> DeleteCustomerAddress(int customerID, string type);
        Task<Address> SaveNewCustomerAddress(int customerID, Address address);
        Task<Address> SetCustomerAddressOnFile(int customerID, Address address, bool isEdit = false);
        Task<bool> SetCustomerPrimaryAddress(int customerID, AddressType type);
       
    }
}
