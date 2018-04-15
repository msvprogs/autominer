using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;
using Msv.AutoMiner.NetworkInfo.Specific;

namespace Msv.AutoMiner.NetworkInfo
{
    public class NetworkInfoProviderFactory : INetworkInfoProviderFactory
    {
        private const string Dgb = "DGB";
        private const string Xmy = "XMY";

        private static readonly IDictionary<string, Type> M_SpecificProviderTypes;
        private static readonly IDictionary<string, Type> M_SpecificMultiAlgoProviderTypes;

        private readonly IWebClient m_OrdinaryClient;
        private readonly IProxiedWebClient m_ProxiedClient;

        static NetworkInfoProviderFactory()
        {
            var specificNetworkProviders = typeof(NetworkInfoProviderFactory).Assembly
                .GetTypes()
                .Where(x => typeof(INetworkInfoProvider).IsAssignableFrom(x)
                            || typeof(IMultiNetworkInfoProvider).IsAssignableFrom(x))
                .Select(x => new
                {
                    Type = x,
                    Attribute = x.GetCustomAttribute<SpecificCoinInfoProviderAttribute>(),
                    IsMultiAlgo = typeof(IMultiNetworkInfoProvider).IsAssignableFrom(x)
                })
                .Where(x => x.Attribute != null)
                .DistinctBy(x => x.Attribute.Symbol)
                .ToArray();

            M_SpecificProviderTypes = specificNetworkProviders
                .Where(x => !x.IsMultiAlgo)
                .ToDictionary(x => x.Attribute.Symbol, x => x.Type);
            M_SpecificMultiAlgoProviderTypes = specificNetworkProviders
                .Where(x => x.IsMultiAlgo)
                .ToDictionary(x => x.Attribute.Symbol, x => x.Type);
        }

        public NetworkInfoProviderFactory(IWebClient ordinaryClient, IProxiedWebClient proxiedClient)
        {
            m_OrdinaryClient = ordinaryClient ?? throw new ArgumentNullException(nameof(ordinaryClient));
            m_ProxiedClient = proxiedClient ?? throw new ArgumentNullException(nameof(proxiedClient));
        }

        public IMultiNetworkInfoProvider CreateMulti(Coin[] coins)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));

            var providers = M_SpecificMultiAlgoProviderTypes
                .Join(coins, x => x.Key, x => x.Symbol, (x, y) => x.Value, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => Activator.CreateInstance(x, m_OrdinaryClient))
                .Cast<IMultiNetworkInfoProvider>()
                .ToArray();
            return new ComboMultiNetworkInfoProvider(providers);
        }

        public string[] GetHardcodedCoins()
            => M_SpecificProviderTypes.Keys
                .OrderBy(x => x)
                .ToArray();

        public string[] GetHardcodedMultiAlgoCoins()
            => M_SpecificMultiAlgoProviderTypes.Keys
                .OrderBy(x => x)
                .ToArray();

        public INetworkInfoProvider Create(Coin coin)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            var externalProvider = CreateExternal(coin);
            if (coin.NodeHost != null && coin.NodeLogin != null && coin.NodePassword != null)
                return new JsonRpcLocalNetworkInfoProvider(
                    new HttpJsonRpcClient(m_OrdinaryClient, coin.NodeHost, coin.NodePort, coin.NodeLogin, coin.NodePassword), coin, externalProvider);
            return externalProvider;
        }

        private INetworkInfoProvider CreateExternal(Coin coin)
        {
            var options = new NetworkInfoProviderOptions
            {
                GetDifficultyFromLastPoWBlock = coin.GetDifficultyFromLastPoWBlock
            };
            switch (coin.NetworkInfoApiType)
            {
                case CoinNetworkInfoApiType.JsonRpc:
                    return null;
                case CoinNetworkInfoApiType.BchainInfo:
                    return new BchainInfoInfoProvider(m_OrdinaryClient, coin.Symbol);
                case CoinNetworkInfoApiType.ChainRadar:
                    return new ChainRadarInfoProvider(m_OrdinaryClient, coin.Symbol);
                case CoinNetworkInfoApiType.ChainzCryptoid:
                    return new ChainzCryptoidInfoProvider(m_ProxiedClient, coin.Symbol);
                case CoinNetworkInfoApiType.Insight:
                    return new InsightInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
                case CoinNetworkInfoApiType.Iquidus:
                    return new IquidusInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl, options);
                case CoinNetworkInfoApiType.IquidusWithPos:
                    return new IquidusWithPosDifficultyInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl, options);
                case CoinNetworkInfoApiType.MinerGate:
                    return new MinerGateInfoProvider(m_OrdinaryClient, coin.Symbol);
                case CoinNetworkInfoApiType.OpenEthereumPool:
                    return new OpenEthereumPoolInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
                case CoinNetworkInfoApiType.ProHashing:
                    return new ProHashingInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiName);
                case CoinNetworkInfoApiType.TheBlockFactory:
                    return new TheBlockFactoryInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiName);
                case CoinNetworkInfoApiType.TheCryptoChat:
                    return new TheCryptoChatInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiName);
                case CoinNetworkInfoApiType.Special:
                    return CreateExternalSpecial(coin);
                case CoinNetworkInfoApiType.Altmix:
                    return new AltmixInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiName);
                case CoinNetworkInfoApiType.EtcExplorer:
                    return new EtcExplorerInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
                case CoinNetworkInfoApiType.UExplorer:
                    return new UExplorerInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
                case CoinNetworkInfoApiType.CryptoCore:
                    return new CryptoCoreInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
                case CoinNetworkInfoApiType.Bulwark:
                    return new BulwarkInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
                default:
                    return new DummyInfoProvider();
            }
        }

        private INetworkInfoProvider CreateExternalSpecial(Coin coin)
        {
            var coinSymbol = coin.Symbol.ToUpperInvariant();
            switch (coinSymbol)
            {
                case Dgb:
                    return new DigiByteInfoProvider(m_OrdinaryClient, coin.Algorithm.KnownValue.GetValueOrDefault());
                case Xmy:
                    return new MyriadCoinInfoProvider(m_OrdinaryClient, coin.Algorithm.KnownValue.GetValueOrDefault());
                default:
                    return M_SpecificProviderTypes.TryGetValue(coinSymbol, out var providerType)
                        ? (INetworkInfoProvider) Activator.CreateInstance(providerType, m_OrdinaryClient)
                        : new DummyInfoProvider();
            }
        }

        private class DummyInfoProvider : INetworkInfoProvider
        {
            public CoinNetworkStatistics GetNetworkStats() 
                => new CoinNetworkStatistics
                {
                    Difficulty = 0,
                    NetHashRate = 0
                };

            public Uri CreateTransactionUrl(string hash)
                => null;

            public Uri CreateAddressUrl(string address)
                => null;

            public Uri CreateBlockUrl(string blockHash)
                => null;
        }
    }
}
