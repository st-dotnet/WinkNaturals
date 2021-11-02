using Exigo.Api.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinkNatural.Common.Utils;
using WinkNatural.Services.Interfaces;

namespace WinkNatural.Services.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ExigoApiClient exigoApiClient = new ExigoApiClient(ExigoConfig.Instance.CompanyKey, ExigoConfig.Instance.LoginName, ExigoConfig.Instance.Password);

        private readonly IConfiguration _config;

        public CustomerService(IConfiguration config)
        {
            _config = config;
        }

        #region public methods

        public async Task<GetCustomersResponse> GetCustomer(int customerId)
        {
            try
            {
                return await exigoApiClient.GetCustomersAsync(new GetCustomersRequest { CustomerID = customerId });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<string> GetImage()
        {
            try
            {
                GetResourceSetCulturesRequest req = new GetResourceSetCulturesRequest();
                var aa = await exigoApiClient.GetResourceSetCulturesAsync(req);
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public Task<UpdateCustomerResponse> UpdateCustomer(UpdateCustomerRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
