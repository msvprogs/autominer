using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface ICoinNetworkInfoUpdater
    {
        Coin[] UpdateNetworkInfo();
    }
}
