using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Contracts
{
    public interface IWalletInfoProviderFactory
    {
        ILocalWalletInfoProvider CreateLocal(Coin coin, WalletBalanceSource balanceSource);
        IExchangeWalletInfoProvider CreateExchange(ExchangeType exchangeType);
    }
}