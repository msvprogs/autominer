using Msv.AutoMiner.FrontEnd.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public interface ICoinValueProvider
    {
        CoinValue[] GetCurrentCoinValues();
    }
}
