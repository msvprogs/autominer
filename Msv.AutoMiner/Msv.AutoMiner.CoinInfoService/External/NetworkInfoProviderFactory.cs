using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.External
{
    public class NetworkInfoProviderFactory : INetworkInfoProviderFactory
    {
        private readonly IWebClient m_OrdinaryClient;
        private readonly IWebClient m_DdosProtectedClient;
        private readonly IWebClient m_DdosProtectedClientWithDelay;

        public NetworkInfoProviderFactory(IWebClient ordinaryClient, IWebClient ddosProtectedClient, IWebClient ddosProtectedClientWithDelay)
        {
            m_OrdinaryClient = ordinaryClient ?? throw new ArgumentNullException(nameof(ordinaryClient));
            m_DdosProtectedClient = ddosProtectedClient ?? throw new ArgumentNullException(nameof(ddosProtectedClient));
            m_DdosProtectedClientWithDelay = ddosProtectedClientWithDelay ?? throw new ArgumentNullException(nameof(ddosProtectedClientWithDelay));
        }

        public IMultiNetworkInfoProvider CreateMulti(Coin[] coins)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));

            return new ComboMultiNetworkInfoProvider(
                new YiimpMultiInfoProvider(
                    m_DdosProtectedClientWithDelay,
                    "https://www.zpool.ca",
                    TimeZoneInfo.CreateCustomTimeZone("GMT-4", TimeSpan.FromHours(-4), "GMT-4", "GMT-4"),
                    coins.Select(x => x.Symbol).Distinct().ToArray()));
        }

        public INetworkInfoProvider Create(Coin coin)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            var externalProvider = CreateExternal(coin);
            if (coin.NodeHost != null && coin.NodeLogin != null && coin.NodePassword != null)
                return new JsonRpcLocalNetworkInfoProvider(
                    new JsonRpcClient(coin.NodeHost, coin.NodePort, coin.NodeLogin, coin.NodePassword), coin, externalProvider);
            return externalProvider;
        }

        private INetworkInfoProvider CreateExternal(Coin coin)
        {
            switch (coin.NetworkInfoApiType)
            {
                case CoinNetworkInfoApiType.JsonRpc:
                    return null;
                case CoinNetworkInfoApiType.BchainInfo:
                    return new BchainInfoInfoProvider(m_OrdinaryClient, coin.Symbol);
                case CoinNetworkInfoApiType.ChainRadar:
                    return new ChainRadarInfoProvider(m_OrdinaryClient, coin.Symbol);
                case CoinNetworkInfoApiType.ChainzCryptoid:
                    return new ChainzCryptoidInfoProvider(m_DdosProtectedClient, coin.Symbol);
                case CoinNetworkInfoApiType.Insight:
                    return new InsightInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
                case CoinNetworkInfoApiType.Iquidus:
                    return new IquidusInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
                case CoinNetworkInfoApiType.IquidusWithPos:
                    return new IquidusWithPosDifficultyInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiUrl);
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
                default:
                    return new DummyInfoProvider();
            }
        }

        private INetworkInfoProvider CreateExternalSpecial(Coin coin)
        {
            switch (coin.Symbol.ToUpperInvariant())
            {
                case "XCN":
                    return new CryptoniteInfoProvider(m_OrdinaryClient);
                case "FTC":
                    return new FeatherCoinInfoProvider(m_OrdinaryClient);
                case "ZEC":
                    return new ZcashInfoProvider(m_OrdinaryClient);
                case "ETC":
                    return new EthereumClassicInfoProvider(m_OrdinaryClient);
                case "PXC":
                    return new PhoenixCoinInfoProvider(m_OrdinaryClient);
                case "DGB":
                    return new DigiByteInfoProvider(m_OrdinaryClient, coin.Algorithm.KnownValue.GetValueOrDefault());
                case "LBC":
                    return new LbryInfoProvider(m_OrdinaryClient);
                case "ETH":
                    return new EthereumInfoProvider(m_OrdinaryClient);
                case "SC":
                    return new SiaCoinInfoProvider(m_OrdinaryClient);
                case "SIB":
                    return new SibCoinInfoProvider(m_OrdinaryClient);
                case "DCR":
                    return new DecredInfoProvider(m_OrdinaryClient);
                case "BTC":
                    return new BitCoinInfoProvider(m_OrdinaryClient);
                case "KMD":
                    return new KomodoInfoProvider(m_OrdinaryClient);
                case "XPM":
                    return new PrimeCoinInfoProvider(m_OrdinaryClient);
                case "PASC":
                    return new PascalCoinInfoProvider(m_OrdinaryClient);
                case "DAXX":
                    return new DaxxCoinInfoProvider(m_OrdinaryClient);
                case "MAX":
                    return new MaxCoinInfoProvider(m_OrdinaryClient);
                case "MNX":
                    return new MinexCoinInfoProvider(m_OrdinaryClient);
                case "HSR":
                    return new HshareInfoProvider(m_OrdinaryClient);
                default:
                    return new DummyInfoProvider();
            }
        }

        private class DummyInfoProvider : INetworkInfoProvider
        {
            public CoinNetworkStatistics GetNetworkStats()
            {
                return new CoinNetworkStatistics
                {
                    Difficulty = 0,
                    NetHashRate = 0
                };
            }
        }
    }
}
