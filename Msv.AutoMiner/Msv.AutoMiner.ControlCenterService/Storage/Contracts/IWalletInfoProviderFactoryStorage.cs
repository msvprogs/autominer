using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage.Contracts
{
    public interface IWalletInfoProviderFactoryStorage
    {
        Exchange GetExchange(ExchangeType type);
    }
}
