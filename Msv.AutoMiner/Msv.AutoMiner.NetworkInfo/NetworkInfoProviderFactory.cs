using System;
using System.Collections.Generic;
using System.Linq;
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
        //Single-algo coins
        private const string Xcn = "XCN";
        private const string Ftc = "FTC";
        private const string Zec = "ZEC";
        private const string Etc = "ETC";
        private const string Dgb = "DGB";
        private const string Lbc = "LBC";
        private const string Eth = "ETH";
        private const string Sc = "SC";
        private const string Sib = "SIB";
        private const string Dcr = "DCR";
        private const string Btc = "BTC";
        private const string Xpm = "XPM";
        private const string Pasc = "PASC";
        private const string Max = "MAX";
        private const string Mnx = "MNX";
        private const string Hsr = "HSR";
        private const string Xmy = "XMY";
        private const string Btg = "BTG";
        private const string Stak = "STAK";
        private const string Elp = "ELP";

        //Multi-algo coins
        private const string Xvg = "XVG";
        private const string Xsh = "XSH";

        private readonly IWebClient m_OrdinaryClient;
        private readonly IProxiedWebClient m_ProxiedClient;

        public NetworkInfoProviderFactory(IWebClient ordinaryClient, IProxiedWebClient proxiedClient)
        {
            m_OrdinaryClient = ordinaryClient ?? throw new ArgumentNullException(nameof(ordinaryClient));
            m_ProxiedClient = proxiedClient ?? throw new ArgumentNullException(nameof(proxiedClient));
        }

        public IMultiNetworkInfoProvider CreateMulti(Coin[] coins)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));

            var providers = new List<IMultiNetworkInfoProvider>();
            if (coins.Any(x => Xvg.Equals(x.Symbol, StringComparison.InvariantCultureIgnoreCase)))
                providers.Add(new VergeMultiNetworkInfoProvider(m_OrdinaryClient));
            if (coins.Any(x => Xsh.Equals(x.Symbol, StringComparison.InvariantCultureIgnoreCase)))
                providers.Add(new ShieldMultiNetworkInfoProvider(m_OrdinaryClient));

            return new ComboMultiNetworkInfoProvider(providers.ToArray());
        }

        public string[] GetHardcodedCoins()
            => new[] {Xcn, Ftc, Zec, Etc, Dgb, Lbc, Eth, Sc, Sib, Dcr, Btc, Xpm, Pasc, Max, Mnx, Hsr, Xmy, Btg, Stak, Elp}
                .OrderBy(x => x)
                .ToArray();

        public string[] GetHardcodedMultiAlgoCoins()
            => new[] {Xvg, Xsh}
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
                default:
                    return new DummyInfoProvider();
            }
        }

        private INetworkInfoProvider CreateExternalSpecial(Coin coin)
        {
            switch (coin.Symbol.ToUpperInvariant())
            {
                case Xcn:
                    return new CryptoniteInfoProvider(m_OrdinaryClient);
                case Ftc:
                    return new FeatherCoinInfoProvider(m_OrdinaryClient);
                case Zec:
                    return new ZcashInfoProvider(m_OrdinaryClient);
                case Etc:
                    return new EthereumClassicInfoProvider(m_OrdinaryClient);
                case Dgb:
                    return new DigiByteInfoProvider(m_OrdinaryClient, coin.Algorithm.KnownValue.GetValueOrDefault());
                case Lbc:
                    return new LbryInfoProvider(m_OrdinaryClient);
                case Eth:
                    return new EthereumInfoProvider(m_OrdinaryClient);
                case Sc:
                    return new SiaCoinInfoProvider(m_OrdinaryClient);
                case Sib:
                    return new SibCoinInfoProvider(m_OrdinaryClient);
                case Dcr:
                    return new DecredInfoProvider(m_OrdinaryClient);
                case Btc:
                    return new BitCoinInfoProvider(m_OrdinaryClient);
                case Xpm:
                    return new PrimeCoinInfoProvider(m_OrdinaryClient);
                case Pasc:
                    return new PascalCoinInfoProvider(m_OrdinaryClient);
                case Max:
                    return new MaxCoinInfoProvider(m_OrdinaryClient);
                case Mnx:
                    return new MinexCoinInfoProvider(m_OrdinaryClient);
                case Hsr:
                    return new HshareInfoProvider(m_OrdinaryClient);
                case Xmy:
                    return new MyriadCoinInfoProvider(m_OrdinaryClient, coin.Algorithm.KnownValue.GetValueOrDefault());
                case Btg:
                    return new BitCoinGoldInfoProvider(m_OrdinaryClient);
                case Stak:
                    return new StraksInfoProvider(m_OrdinaryClient);
                case Elp:
                    return new ElleriumInfoProvider(m_OrdinaryClient);
                default:
                    return new DummyInfoProvider();
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
