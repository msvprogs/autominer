using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage.Contracts
{
    public interface IWalletInfoProviderFactoryStorage
    {
        Exchange GetExchange(ExchangeType type);
    }
}
