using Exigo.Api.Client;
using System.Collections.Generic;

namespace WinkNatural.Web.Services.DTO.Shopping.CreateOrder
{
    public class CreateOrderRequestModel
    {
        public CreateOrderRequest createOrderRequest { get; set; }
        public List<OrderDetailRequest> orderDetailRequests { get; set; }
    }
}
