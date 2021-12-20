using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WinkNaturals.Infrastructure.Services.ExigoService
{
    public class VerifyAddressResponse
    {
        public bool IsValid { get; set; }
        public IAddress OriginalAddress { get; set; }
        public IAddress VerifiedAddress { get; set; }
    }
}
