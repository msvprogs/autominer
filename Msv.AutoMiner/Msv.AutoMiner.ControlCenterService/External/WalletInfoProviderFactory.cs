using System;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.External
{
    public class WalletInfoProviderFactory : IWalletInfoProviderFactory
    {
        private readonly IWebClient m_WebClient;
        private readonly Func<IWalletInfoProviderFactoryStorage> m_StorageGetter;

        public WalletInfoProviderFactory(IWebClient webClient, Func<IWalletInfoProviderFactoryStorage> storageGetter)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        public IWalletInfoProvider CreateLocal(Coin coin)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            return new JsonRpcLocalWalletInfoProvider(
                new JsonRpcClient(coin.NodeHost, coin.NodePort, coin.NodeLogin, coin.NodePassword),
                coin);
        }

        public IWalletInfoProvider CreateExchange(ExchangeType exchangeType)
        {
            var exchange = m_StorageGetter.Invoke().GetExchange(exchangeType);
            if (exchange?.PrivateKey == null || exchange.PublicKey == null)
                return new DummyWalletInfoProvider();
            var privateKey = exchange.PrivateKey;
            switch (exchangeType)
            {
                case ExchangeType.Bittrex:
                    return new BittrexWalletInfoProvider(m_WebClient, exchange.PublicKey, privateKey);
                case ExchangeType.Cryptopia:
                    return new CryptopiaWalletInfoProvider(m_WebClient, exchange.PublicKey, privateKey);
                case ExchangeType.Poloniex:
                    return new PoloniexWalletInfoProvider(m_WebClient, exchange.PublicKey, privateKey);
                case ExchangeType.YoBit:
                    return new YoBitWalletInfoProvider(m_WebClient, exchange.PublicKey, privateKey);
                case ExchangeType.TradeSatoshi:
                    return new TradeSatoshiWalletInfoProvider(m_WebClient, exchange.PublicKey, privateKey);
                case ExchangeType.Novaexchange:
                    return new NovaexchangeWalletInfoProvider(m_WebClient, exchange.PublicKey, privateKey);
                case ExchangeType.StocksExchange:
                    return new StocksExchangeWalletInfoProvider(m_WebClient, exchange.PublicKey, privateKey);
                default:
                    return new DummyWalletInfoProvider();
            }
        }

        private class DummyWalletInfoProvider : IWalletInfoProvider
        {
            public WalletBalanceData[] GetBalances()
                => new WalletBalanceData[0];

            public WalletOperationData[] GetOperations(DateTime startDate)
                => new WalletOperationData[0];
        }
    }
}
