using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Infrastructure.Services.ExigoService;
using static WinkNaturals.Helpers.Constant;

namespace WinkNaturals.Infrastructure.Services
{
    public class GetCreditCardResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
