namespace WinkNaturals.Models.Shopping.Interfaces
{
    public interface IMarketConfiguration
    {
        MarketName MarketName { get; }

        IOrderConfiguration Orders { get; }

    }
}
