using System;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Exchanges.Api;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public abstract class ExchangeWalletInfoProviderBase : IWalletInfoProvider
    {
        protected IExchangeApi Api { get; }
        protected string ApiKey { get; }
        protected byte[] ApiSecret { get; }

        protected ExchangeWalletInfoProviderBase(IExchangeApi api, string apiKey, byte[] apiSecret)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Value cannot be null or empty.", nameof(apiKey));
            if (apiSecret == null)
                throw new ArgumentNullException(nameof(apiSecret));
            if (apiSecret.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(apiSecret));

            Api = api ?? throw new ArgumentNullException(nameof(api));
            ApiKey = apiKey;
            ApiSecret = apiSecret;
        }

        public abstract WalletBalanceData[] GetBalances();
        public abstract WalletOperationData[] GetOperations(DateTime startDate);
    }
}
