using Msv.AutoMiner.CoinInfoService.External.Data;

namespace Msv.AutoMiner.CoinInfoService.External.Contracts
{
    public interface IMasternodeInfoProviderFactory
    {
        IMasternodeInfoProvider Create(MasternodeInfoSource source);
    }
}
