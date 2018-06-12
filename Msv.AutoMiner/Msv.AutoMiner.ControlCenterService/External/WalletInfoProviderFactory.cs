using System;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Exchanges.Api;
using Msv.AutoMiner.NetworkInfo;

namespace Msv.AutoMiner.ControlCenterService.External
{
    public class WalletInfoProviderFactory : IWalletInfoProviderFactory
    {
        private readonly IWebClient m_WebClient;
        private readonly ISessionedRpcClientFactory m_SessionedRpcClientFactory;
        private readonly INetworkInfoProviderFactory m_NetworkInfoProviderFactory;
        private readonly Func<IWalletInfoProviderFactoryStorage> m_StorageGetter;

        public WalletInfoProviderFactory(
            IWebClient webClient,
            ISessionedRpcClientFactory sessionedRpcClientFactory,
            INetworkInfoProviderFactory networkInfoProviderFactory,
            Func<IWalletInfoProviderFactoryStorage> storageGetter)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_SessionedRpcClientFactory = sessionedRpcClientFactory ?? throw new ArgumentNullException(nameof(sessionedRpcClientFactory));
            m_NetworkInfoProviderFactory = networkInfoProviderFactory ?? throw new ArgumentNullException(nameof(networkInfoProviderFactory));
            m_StorageGetter = storageGetter ?? throw new ArgumentNullException(nameof(storageGetter));
        }

        public ILocalWalletInfoProvider CreateLocal(Coin coin, WalletBalanceSource balanceSource)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            switch (balanceSource)
            {
                case WalletBalanceSource.LocalNode when coin.NodeHost != null && coin.NodeLogin != null && coin.NodePassword != null:
                    return new JsonRpcLocalWalletInfoProvider(
                        new HttpJsonRpcClient(m_WebClient, coin.NodeHost, coin.NodePort, coin.NodeLogin, coin.NodePassword),
                        coin);
                case WalletBalanceSource.BlockExplorer:
                    return new BlockExplorerLocalWalletInfoProvider(coin.Symbol, m_NetworkInfoProviderFactory.Create(coin));
                default:
                    throw new ArgumentException($"Invalid wallet balance source: {balanceSource}");
            }
        }

        public IExchangeWalletInfoProvider CreateExchange(ExchangeType exchangeType)
        {
            var exchange = m_StorageGetter.Invoke().GetExchange(exchangeType);
            if (exchange?.PrivateKey == null || exchange.PublicKey == null)
                return new DummyWalletInfoProvider();
            switch (exchangeType)
            {
                case ExchangeType.Bittrex:
                    return new BittrexWalletInfoProvider(
                        new BittrexExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.Cryptopia:
                    return new CryptopiaWalletInfoProvider(
                        new CryptopiaExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.Poloniex:
                    return new PoloniexWalletInfoProvider(
                        new PoloniexExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.YoBit:
                    return new YoBitWalletInfoProvider(
                        new YoBitExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.TradeSatoshi:
                    return new TradeSatoshiWalletInfoProvider(
                        new TradeSatoshiExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.Novaexchange:
                    return new NovaexchangeWalletInfoProvider(
                        new NovaexchangeExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.StocksExchange:
                    return new StocksExchangeWalletInfoProvider(
                        new StocksExchangeExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.LiveCoin:
                    return new LiveCoinWalletInfoProvider(
                        new LiveCoinExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.BtcAlpha:
                    return new BtcAlphaWalletInfoProvider(
                        new BtcAlphaExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                case ExchangeType.CryptoBridge:
                    return new CryptoBridgeWalletInfoProvider(m_SessionedRpcClientFactory, exchange.PublicKey);
                case ExchangeType.Graviex:
                    return new GraviexWalletInfoProvider(
                        new GraviexExchangeApi(m_WebClient), exchange.PublicKey, exchange.PrivateKey);
                default:
                    return new DummyWalletInfoProvider();
            }
        }

        private class DummyWalletInfoProvider : IExchangeWalletInfoProvider
        {
            public WalletBalanceData[] GetBalances()
                => new WalletBalanceData[0];

            public WalletOperationData[] GetOperations(DateTime startDate)
                => new WalletOperationData[0];
        }
    }
}
