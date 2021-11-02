using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exigo.Api.Client;

namespace WinkNatural.Services.DTO.Shopping.CreateOrder
{
    public class CreateOrderRequestModel
    {
       public CreateOrderRequest createOrderRequest { get; set; }
       public List<OrderDetailRequest> orderDetailRequests { get; set; }     
    }
}
