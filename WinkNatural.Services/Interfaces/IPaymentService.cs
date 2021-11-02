using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Services.DTO;
using WinkNatural.Services.DTO.Shopping;
using WinkNatural.Services.Services;
using WinkNaturals.Models;


namespace WinkNatural.Services.Interfaces
{
    public interface IPaymentService
    {
        BankCardTransaction ProcessPayment(WinkPaymentRequest winkPaymentRequest);

        Task<AddCardResponse> CreateCustomerProfile(GetPaymentRequest model);

        ProcessPaymentMethodTransactionResponse ProcessPaymentMethod(GetPaymentRequest getPaymentProPayModel);

        Task<AddCardResponse> AddPayment(AddPaymentModel model);

    }
}
