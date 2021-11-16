using Exigo.Api.Client;

namespace WinkNaturals.Setting.Interfaces
{
    public interface IExigoApiContext
    {
        public ExigoApiClient GetContext();
    }
}
