using Msv.AutoMiner.CoinInfoService.External.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts
{
    public interface IMasternodeInfoStorage
    {
        void Store(MasternodeInfo[] infos);
        MasternodeInfo Load(string currencySymbol);
    }
}
