using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface IWalletInfoProviderFactory
    {
        IWalletInfoProvider CreateLocal(Coin coin);
        IWalletInfoProvider CreateExchange(ExchangeType exchangeType);
    }
}