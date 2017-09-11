using System;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    public abstract class ExchangeTraderBase : WebDownloaderBase, IExchangeTrader
    {
        protected string ApiKey { get; }
        protected byte[] ApiSecret { get; }

        protected ExchangeTraderBase(string apiKey, byte[] apiSecret)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Value cannot be null or empty.", nameof(apiKey));
            if (apiSecret == null)
                throw new ArgumentNullException(nameof(apiSecret));
            if (apiSecret.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(apiSecret));

            ApiKey = apiKey;
            ApiSecret = apiSecret;
        }

        public abstract ExchangeAccountBalanceData[] GetBalances();
        public abstract ExchangeAccountOperationData[] GetOperations(DateTime startDate);
    }
}
