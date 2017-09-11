using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface IExchangeKeyStorage
    {
        void StoreKey(ExchangeType exchangeType, string publicKey, byte[] privateKey);
    }
}
