using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WinkNaturals.Models;

namespace WinkNaturals.Infrastructure.Services.Interfaces
{
    public interface IEmailService
    {
        public Task<EmailResponse> Send(EmailRequest request);
    }
}
