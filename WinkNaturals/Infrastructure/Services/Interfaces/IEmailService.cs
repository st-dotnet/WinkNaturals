using System.Threading.Tasks;
using WinkNaturals.Models;

namespace WinkNaturals.Infrastructure.Services.Interfaces
{
    public interface IEmailService
    {
        public Task<EmailResponse> Send(EmailRequest request);
    }
}
