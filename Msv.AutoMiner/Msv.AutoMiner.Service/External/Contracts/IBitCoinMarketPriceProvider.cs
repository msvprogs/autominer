namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface IBitCoinMarketPriceProvider
    {
        double GetCurrentPriceUsd();
    }
}
