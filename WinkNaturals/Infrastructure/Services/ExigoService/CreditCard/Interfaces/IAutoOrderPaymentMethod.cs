using Exigo.Api.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.Interfaces
{
    public interface IAutoOrderPaymentMethod
    {
        int[] AutoOrderIDs { get; set; }

        bool IsUsedInAutoOrders { get; }
        AutoOrderPaymentType AutoOrderPaymentType { get; }
    }
}
