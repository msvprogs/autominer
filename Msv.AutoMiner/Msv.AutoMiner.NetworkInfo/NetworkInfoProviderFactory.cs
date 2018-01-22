﻿using System;
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
            if (coins.Any(x => x.Symbol == "XVG"))
                providers.Add(new VergeMultiNetworkInfoProvider(m_OrdinaryClient));
            if (coins.Any(x => x.Symbol == "XSH"))
                providers.Add(new ShieldMultiNetworkInfoProvider(m_OrdinaryClient));

            return new ComboMultiNetworkInfoProvider(providers.ToArray());
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
                    return new ChainzCryptoidInfoProvider(m_ProxiedClient, coin.Symbol);
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
                case CoinNetworkInfoApiType.Altmix:
                    return new AltmixInfoProvider(m_OrdinaryClient, coin.NetworkInfoApiName);
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
                case "XPM":
                    return new PrimeCoinInfoProvider(m_OrdinaryClient);
                case "PASC":
                    return new PascalCoinInfoProvider(m_OrdinaryClient);
                case "MAX":
                    return new MaxCoinInfoProvider(m_OrdinaryClient);
                case "MNX":
                    return new MinexCoinInfoProvider(m_OrdinaryClient);
                case "HSR":
                    return new HshareInfoProvider(m_OrdinaryClient);
                case "XMY":
                    return new MyriadCoinInfoProvider(m_OrdinaryClient, coin.Algorithm.KnownValue.GetValueOrDefault());
                case "BTG":
                    return new BitCoinGoldInfoProvider(m_OrdinaryClient);
                case "STAK":
                    return new StraksInfoProvider(m_OrdinaryClient);
                case "VLT":
                    return new VeltorInfoProvider(m_OrdinaryClient);
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