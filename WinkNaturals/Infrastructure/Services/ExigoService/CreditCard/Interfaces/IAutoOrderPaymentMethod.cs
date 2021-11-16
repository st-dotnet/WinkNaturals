using Exigo.Api.Client;

namespace WinkNaturals.Infrastructure.Services.ExigoService.CreditCard.Interfaces
{
    public interface IAutoOrderPaymentMethod
    {
        int[] AutoOrderIDs { get; set; }

        bool IsUsedInAutoOrders { get; }
        AutoOrderPaymentType AutoOrderPaymentType { get; }
    }
}
