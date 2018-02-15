using System;
using Msv.AutoMiner.Common.Data.Enums;
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
        private readonly ISessionedRpcClientFactory m_SessionedRpcClientFactory;
        private readonly Func<IWalletInfoProviderFactoryStorage> m_StorageGetter;

        public WalletInfoProviderFactory(
            IWebClient webClient,
            ISessionedRpcClientFactory sessionedRpcClientFactory,
            Func<IWalletInfoProviderFactoryStorage> storageGetter)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_SessionedRpcClientFactory = sessionedRpcClientFactory ?? throw new ArgumentNullException(nameof(sessionedRpcClientFactory));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        public IWalletInfoProvider CreateLocal(Coin coin)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            return new JsonRpcLocalWalletInfoProvider(
                new HttpJsonRpcClient(m_WebClient, coin.NodeHost, coin.NodePort, coin.NodeLogin, coin.NodePassword),
                coin);
        }

        public IWalletInfoProvider CreateExchange(ExchangeType exchangeType)
        {
            var exchange = m_StorageGetter.Invoke().GetExchange(exchangeType);
            if (exchange?.PrivateKey == null || exchange.PublicKey == null)
                return new DummyWalletInfoProvider();
            switch (exchangeType)
            {
                case ExchangeType.Bittrex:
                    return new BittrexWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.Cryptopia:
                    return new CryptopiaWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.Poloniex:
                    return new PoloniexWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.YoBit:
                    return new YoBitWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.TradeSatoshi:
                    return new TradeSatoshiWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.Novaexchange:
                    return new NovaexchangeWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.StocksExchange:
                    return new StocksExchangeWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.LiveCoin:
                    return new LiveCoinWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.BtcAlpha:
                    return new BtcAlphaWalletInfoProvider(m_WebClient, exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.CryptoBridge:
                    return new CryptoBridgeWalletInfoProvider(m_SessionedRpcClientFactory, exchange.PublicKey);
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
