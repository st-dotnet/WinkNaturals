using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNatural.Web.Common.Utils;
using WinkNatural.Web.Services.Interfaces;
using WinkNaturals.Helpers;
using WinkNaturals.Infrastructure.Services.DTO;
using WinkNaturals.Infrastructure.Services.Interfaces;
using WinkNaturals.Models.Shopping.Orders;
using WinkNaturals.Setting.Interfaces;

namespace WinkNatural.Web.Services.Services
{
    public class PartyService : IPartyService
    {
        private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);
        private readonly IShoppingService _shoppingService;
        private readonly IExigoApiContext _exigoApiContext;
        private readonly IAccountService _accountService;

        public PartyService(IShoppingService shoppingService, IExigoApiContext exigoApiContext, IAccountService accountService)
        {
            _shoppingService = shoppingService;
            _exigoApiContext = exigoApiContext;
            _accountService = accountService;
        }

        /// <summary>
        /// To Create Party
        /// </summary>
        /// <param name="createPartyRequest"></param>
        public async Task<CreatePartyResponse> CreateParty(CreatePartyRequest createPartyRequest)
        {
            var res = new CreatePartyResponse();
            try
            {
                // Create Request
                var req = new CreatePartyRequest();
                req.PartyType = createPartyRequest.PartyType;              //Party type
                req.PartyStatusType = createPartyRequest.PartyStatusType;        //Party Status
                req.HostID = createPartyRequest.HostID;
                req.DistributorID = createPartyRequest.DistributorID;
                req.StartDate = createPartyRequest.StartDate;
                req.CloseDate = createPartyRequest.CloseDate;              //Close Date
                req.Description = createPartyRequest.Description;          //Description. Must be 100 characters or less.
                req.EventStart = createPartyRequest.EventStart;             //Event Start date
                req.EventEnd = createPartyRequest.EventEnd;               //Event End date
                req.LanguageID = createPartyRequest.LanguageID;             //Language ID
                req.Information = createPartyRequest.Information;          //Information
                req.BookingPartyID = createPartyRequest.BookingPartyID;         //BookingPartyID
                req.Field1 = createPartyRequest.Field1;               //Field1
                req.Field2 = createPartyRequest.Field2;               //Field2
                req.Field3 = createPartyRequest.Field3;               //Field3
                req.Field4 = createPartyRequest.Field4;               //Field4
                req.Field5 = createPartyRequest.Field5;               //Field5

                // Send Request to Server and Get Response
                res = await exigoApiClient.CreatePartyAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Update Parties
        /// </summary>
        /// <param name="updatePartyRequest"></param>
        public async Task<UpdatePartyResponse> UpdateParty(UpdatePartyRequest updatePartyRequest)
        {
            var res = new UpdatePartyResponse();
            try
            {
                // Create Request
                var req = new UpdatePartyRequest();
                req.PartyID = updatePartyRequest.PartyID;
                req.PartyType = updatePartyRequest.PartyType;              //PartyTy
                req.PartyStatusType = updatePartyRequest.PartyStatusType;        //PartyStatusTy
                req.HostID = updatePartyRequest.HostID;                 //HostID
                req.DistributorID = updatePartyRequest.DistributorID;          //DistributorID
                req.StartDate = updatePartyRequest.StartDate;              //StartDate
                req.CloseDate = updatePartyRequest.CloseDate;              //Close Date
                req.Description = updatePartyRequest.Description;          //Description
                req.EventStart = updatePartyRequest.EventStart;             //Event Start date
                req.EventEnd = updatePartyRequest.EventEnd;               //Event End date
                req.LanguageID = updatePartyRequest.LanguageID;             //Language ID
                req.Information = updatePartyRequest.Information;          //Information
                req.BookingPartyID = updatePartyRequest.BookingPartyID;         //BookingPartyID
                req.Field1 = updatePartyRequest.Field1;               //Field1
                req.Field2 = updatePartyRequest.Field2;               //Field2
                req.Field3 = updatePartyRequest.Field3;               //Field3
                req.Field4 = updatePartyRequest.Field4;               //Field4
                req.Field5 = updatePartyRequest.Field5;               //Field5

                //Send Request to Server and Get Response
                res = await exigoApiClient.UpdatePartyAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Get Parties
        /// </summary>
        /// <param name="getPartiesRequest"></param>
        public async Task<GetPartiesResponse> GetParties(GetPartiesRequest getPartiesRequest)
        {
            var res = new GetPartiesResponse();
            try
            {
                // Create Request
                var req = new GetPartiesRequest();
                req.PartyID = getPartiesRequest.PartyID;
                req.HostID = getPartiesRequest.HostID;
                req.DistributorID = getPartiesRequest.DistributorID;
                req.PartyStatusType = getPartiesRequest.PartyStatusType;        //Party Status
                req.BookingPartyID = getPartiesRequest.BookingPartyID;         //BookingPartyID
                req.Field1 = getPartiesRequest.Field1;               //Field1
                req.Field2 = getPartiesRequest.Field2;               //Field2
                req.Field3 = getPartiesRequest.Field3;               //Field3
                req.Field4 = getPartiesRequest.Field4;               //Field4
                req.Field5 = getPartiesRequest.Field5;               //Field5

                //Send Request to Server and Get Response
                res = await exigoApiClient.GetPartiesAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Get Party Guests
        /// </summary>
        /// <param name="getPartyGuestsRequest"></param>
        public async Task<GetPartyGuestsResponse> GetPartyGuests(GetPartyGuestsRequest getPartyGuestsRequest)
        {
            var res = new GetPartyGuestsResponse();
            try
            {
                // Create Request
                var req = new GetPartyGuestsRequest();
                req.PartyID = getPartyGuestsRequest.PartyID;                //The party's unique identifier

                // Send Request to Server and Get Response
                res = await exigoApiClient.GetPartyGuestsAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        /// <summary>
        /// To Creat Guest
        /// </summary>
        /// <param name="createGuestRequest"></param>
        public async Task<CreateGuestResponse> CreateGuest(CreateGuestRequest createGuestRequest)
        {
            var res = new CreateGuestResponse();
            try
            {
                // Create Request
                var req = new CreateGuestRequest();
                req.HostID = createGuestRequest.HostID;                 //The unique identifier of the host that the guest was created/referred by
                req.PartyID = createGuestRequest.PartyID;                //If set, the guest will be placed in the provided party
                req.CustomerID = createGuestRequest.CustomerID;             //If set, the guest will be linked to the provided customer account
                req.FirstName = createGuestRequest.FirstName;         //The guest's first name
                req.LastName = createGuestRequest.LastName;           //The guest's last name
                req.Company = createGuestRequest.Company;     //The guest's company name
                req.GuestStatus = createGuestRequest.GuestStatus;            //The guest's status as defined by the company. Defaults to 1.
                req.Address1 = createGuestRequest.Address1;
                req.State = createGuestRequest.State;               //The state of the guest's address. This field is required if Address1 is provided.
                req.Country = createGuestRequest.Country;             //The country of the guest's address. This field is required if Address1 is provided.
                req.Email = createGuestRequest.Email;
                req.CustomerKey = createGuestRequest.CustomerKey;          // If set, the guest will be linked to the provided customer account

                // Send Request to Server and Get Response
                res = await exigoApiClient.CreateGuestAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }
        /// <summary>
        /// To Update Guest
        /// </summary>
        /// <param name="createGuestRequest"></param>
        public async Task<UpdateGuestResponse> UpdateGuest(UpdateGuestRequest updateGuestRequest)
        {
            var res = new UpdateGuestResponse();
            try
            {
                // Create Request
                var req = new UpdateGuestRequest();
                req.GuestID = updateGuestRequest.GuestID;                //The unique identifier of the guest
                req.CustomerID = updateGuestRequest.CustomerID;             //Unique numeric identifier for customer record.
                req.CustomerKey = updateGuestRequest.CustomerKey;          //Unique alpha numeric identifier for customer record. Exeption will occur if CustomerID & CustomerKey are provided.
                req.FirstName = updateGuestRequest.FirstName;
                req.MiddleName = updateGuestRequest.MiddleName;
                req.LastName = updateGuestRequest.LastName;
                req.NameSuffix = updateGuestRequest.NameSuffix;
                req.Company = updateGuestRequest.Company;
                req.GuestStatus = updateGuestRequest.GuestStatus;
                req.Address1 = updateGuestRequest.Address1;
                req.Address2 = updateGuestRequest.Address2;
                req.Address3 = updateGuestRequest.Address3;
                req.City = updateGuestRequest.City;
                req.County = updateGuestRequest.County;
                req.Zip = updateGuestRequest.Zip;
                req.Phone = updateGuestRequest.Phone;
                req.Phone2 = updateGuestRequest.Phone2;
                req.MobilePhone = updateGuestRequest.MobilePhone;
                req.Email = updateGuestRequest.Email;
                req.Date1 = updateGuestRequest.Date1;
                req.Date2 = updateGuestRequest.Date2;
                req.Date3 = updateGuestRequest.Date3;
                req.Date4 = updateGuestRequest.Date4;
                req.Date5 = updateGuestRequest.Date5;
                req.Notes = updateGuestRequest.Notes;

                // Send Request to Server and Get Response
                res = await exigoApiClient.UpdateGuestAsync(req);
            }
            catch (Exception e)
            {
                e.Message.ToString();
            }
            return res;
        }

        public async Task<TransactionalResponse> ManageAutoOrder(ManageAutoOrderViewModel autoOrderViewModel, int id)
        {
            int arraySize = 4;
            Exigo.Api.Client.TransactionalResponse response = new();
            Exigo.Api.Client.TransactionalRequest request = new()
            {
                TransactionRequests = new ITransactionMember[arraySize]
            };
            try
            {
                var customerID = id;
                var customer = _shoppingService.GetCustomer(customerID);
                var market = "US";
                var configuration = "US";
                var warehouseID = 1;
                var isExistingAutoOrder = id != 0;
                var paymentMethods = _accountService.GetCustomerBilling(id);

                autoOrderViewModel.AutoOrder.StartDate = autoOrderViewModel.AutoOrder.StartDate < DateTime.Now.ToCST() ? DateTime.Now.ToCST() : autoOrderViewModel.AutoOrder.StartDate;
                autoOrderViewModel.AutoOrder.Details = autoOrderViewModel.AutoOrder.Details.Where(d => d.Quantity > 0).ToList();
                if (!autoOrderViewModel.AutoOrder.Details.Any())
                {
                }
                //autoOrderViewModel.AvailableProducts = _shoppingService.GetItems(new GetItemsRequestAutoOrder()
                //{
                //    Configuration = "DS",
                //    LanguageID = _orderConfiguration.LanguageID,
                //    ItemCodes = autoOrderViewModel.AutoOrder.Details.Select(x => x.ItemCode).ToArray(),
                //}).OrderBy(c => c.SortOrder).ToList();
                foreach (var x in autoOrderViewModel.AutoOrder.Details)
                {
                    if (!isExistingAutoOrder)
                    {
                        x.PriceEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
                        x.TaxableEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
                        x.ShippingPriceEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
                        x.BusinessVolumeEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.BV).FirstOrDefault();
                        x.CommissionableVolumeEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.CV).FirstOrDefault();
                    }
                    else if (x.PriceEachOverride == null)
                    {
                        x.PriceEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
                        x.TaxableEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
                        x.ShippingPriceEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.Price).FirstOrDefault();
                        x.BusinessVolumeEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.BV).FirstOrDefault();
                        x.CommissionableVolumeEachOverride = autoOrderViewModel.AvailableProducts.Where(y => y.ItemCode == x.ItemCode).Select(y => y.CV).FirstOrDefault();
                    }
                }
                // Save New Credit Card
                var isUsingNewCard = autoOrderViewModel.AutoOrder.AutoOrderPaymentTypeID == 0;
                var hasPrimaryCard = paymentMethods.Result.Where(v => v.IsComplete).Count() > 0;
                if (isUsingNewCard)
                {
                    var saveCCRequest = new SetAccountCreditCardTokenRequest();

                    // If there is one or more available payment type, save the card in the secondary card slot
                    if (hasPrimaryCard)
                    {
                        saveCCRequest.CreditCardAccountType = AccountCreditCardType.Secondary;
                        autoOrderViewModel.AutoOrder.AutoOrderPaymentTypeID = AutoOrderPaymentTypes.SecondaryCreditCardOnFile;
                    }
                    else
                    {
                        autoOrderViewModel.AutoOrder.AutoOrderPaymentTypeID = AutoOrderPaymentTypes.PrimaryCreditCardOnFile;
                    }
                    saveCCRequest.CustomerID = customerID;
                    request.TransactionRequests[3] = saveCCRequest;
                }
                // ToDo:  Joshua Remove after all users converted to TokenEx
                if (!hasPrimaryCard)
                {
                    var updateCustomerRequest = new UpdateCustomerRequest
                    {
                        CustomerID = customerID,
                        Field1 = "1"
                    };
                    var transactionResponse = _shoppingService.UpdateCustomer(updateCustomerRequest);
                }
                else
                {
                    var updateCustomerRequest = new UpdateCustomerRequest
                    {
                        CustomerID = customerID,
                        Field2 = "1"
                    };
                    var transactionResponse = _shoppingService.UpdateCustomer(updateCustomerRequest);
                }
                // Prepare the auto order
                var autoOrder = autoOrderViewModel.AutoOrder;
                var createAutoOrderRequest = new CreateAutoOrderRequest()
                {
                    PriceType = 1,
                    WarehouseID = warehouseID,
                    Notes = !string.IsNullOrEmpty(autoOrder.Notes)
                                    ? autoOrder.Notes
                                    : string.Format("Created with the API Auto-Delivery manager at \"{0}\" on {1:u} at IP {2} using {3} {4} ({5}).",
                                        DateTime.Now.ToUniversalTime()
                                      ),
                    CustomerID = customerID
                };
                request.TransactionRequests[2] = createAutoOrderRequest;

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
    }
}
