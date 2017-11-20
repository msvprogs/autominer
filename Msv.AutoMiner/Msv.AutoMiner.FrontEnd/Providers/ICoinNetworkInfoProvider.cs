using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public interface ICoinNetworkInfoProvider
    {
        CoinNetworkInfo[] GetCurrentNetworkInfos();
    }
}
