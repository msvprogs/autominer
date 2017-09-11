using System;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.External.Network;
using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External
{
    public class CoinNetworkInfoProviderFactory : ICoinNetworkInfoProviderFactory
    {
        private static readonly string[] M_ZpoolCurrencies =
            {"VLT", "CHC", "BLC", "LIT", "UMO", "PHO", "ORB", "BOAT", "XVG", "J", "XRE"};

        public IMultiCoinNetworkInfoProvider CreateMulti(Coin[] coins, IDDoSTriggerPreventingDownloader downloader)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));
            if (downloader == null)
                throw new ArgumentNullException(nameof(downloader));

            return new ComboMultiCoinInfoProvider(
                new YiimpMultiInfoProvider(
                    downloader,
                    "https://www.zpool.ca",
                    TimeZoneInfo.CreateCustomTimeZone("GMT-4", TimeSpan.FromHours(-4), "GMT-4", "GMT-4"),
                    coins.Select(x => x.CurrencySymbol).Intersect(M_ZpoolCurrencies).ToArray()));
        }

        public ICoinNetworkInfoProvider Create(Coin coin)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));
            if (coin.UseLocalNetworkInfo)
                return new JsonRpcLocalNetworkInfoProvider(coin);
            var currencySymbol = coin.CurrencySymbol.ToUpperInvariant();
            switch (currencySymbol)
            {
                case "XCN":
                    return new CryptoniteInfoProvider();
                case "FTC": 
                    return new FeatherCoinInfoProvider();
                case "ZCL": 
                    return new InsightInfoProvider("http://zclexplorer.org/api");
                case "ZEC": 
                    return new ZcashInfoProvider();
                case "ETC": 
                    return new EthereumClassicInfoProvider();
                case "BCN": 
                    return new ByteCoinInfoProvider();
                case "GRS":
                case "BTX":
                case "MAC":
                case "NETKO":
                case "QRK":
                case "FLAX":
                case "DMD": 
                case "NEVA":
                case "UFO":
                case "GUN":
                case "VUC":
                case "XLR":
                    return new ChainzCryptoidInfoProvider(currencySymbol);
                case "PXC": 
                    return new PhoenixCoinInfoProvider();
                case "DGB": 
                    return new DigiByteInfoProvider(coin.Algorithm);
                case "LBC": 
                    return new LbryInfoProvider();
                case "ETH": 
                    return new EthereumInfoProvider();
                case "VTC": 
                    return new BchainInfoInfoProvider(currencySymbol);
                case "SC":  
                    return new SiaCoinInfoProvider();
                case "MONA": 
                    return new MonaCoinInfoProvider();
                case "SIB":  
                    return new SibCoinInfoProvider();
                case "MUSIC":
                    return new OpenEthereumPoolInfoProvider("http://149.202.51.96:8080/api/stats");
                case "UBQ":
                    return new OpenEthereumPoolInfoProvider("http://ubiqpool.io/api/stats");
                case "EXP":
                    return new OpenEthereumPoolInfoProvider("http://52.165.128.189/api/stats");
                case "UR":
                    return new OpenEthereumPoolInfoProvider("http://ur.poolcoin.biz/apiur/stats");
                case "DCR": 
                    return new DecredInfoProvider();
                case "KRB":
                    return new KarbowanecInfoProvider();
                case "XMR": 
                    return new MoneroInfoProvider();
                case "BTC": 
                    return new BitCoinInfoProvider();
                case "WYV":
                    // 02.07.2017 - unavailable
                    // return new IquidusWithNonNumericDifficultyInfoProvider("http://explorer.projectwyvern.com:3001");
                    return new ChainzCryptoidInfoProvider(currencySymbol);
                case "KMD": 
                    return new KomodoInfoProvider();
                case "START": 
                    return new InsightInfoProvider("http://explorer.startcoin.org/api");
                case "ADZ": 
                    return new IquidusInfoProvider("http://adzcoin.net:3001");
                case "SWEEP": 
                    return new IquidusInfoProvider("http://sweepstakecoin.info:3001/");
                case "SIGT":
                    return new IquidusInfoProvider("http://explorer.signatum.download");
                case "NAMO":
                    return new IquidusInfoProvider("http://namocoin.dynns.com:3001/");
                case "CTIC2":
                    return new Coimatic2InfoProvider();
                case "DNR": 
                    return new IquidusWithNonNumericDifficultyInfoProvider("http://denarius.name:3001");
                case "XZC": 
                    return new ZCoinInfoProvider();
                case "MYR": 
                    return new ProHashingInfoProvider("Myriadcoin");
                case "XVC": 
                    return new VCashInfoProvider();
                case "XPM": 
                    return new PrimeCoinInfoProvider();
                case "ZEN":
                    return new InsightInfoProvider("https://explorer.zensystem.io/insight-api-zen");
                case "PASL":
                    return new PascalLiteInfoProvider();
                case "PASC":
                    return new PascalCoinInfoProvider();
                case "MAX":
                    return new SwaggerInfoProvider("http://api.maxcoinhub.io");
                case "BTPL":
                    return new TheCryptoChatInfoProvider("bitcoinplanet");
                case "INFO":
                    return new TheCryptoChatInfoProvider("infocoin");
                case "HONEY":
                    return new TheCryptoChatInfoProvider("honey");
                case "DAXX":
                    return new DaxxCoinInfoProvider();
                case "CXT":
                    return new IquidusWithNonNumericDifficultyInfoProvider("http://194.135.95.46:3001/");
                case "MGT":
                    return new IquidusInfoProvider("http://explorer.magnatum.io/");
                case "VIVO":
                    return new IquidusInfoProvider("http://vivo.explorerz.top:3003/");
                case "REC":
                    return new TheCryptoChatInfoProvider("royalempire");
                case "ALTCOM":
                    return new IquidusWithNonNumericDifficultyInfoProvider("https://altcom.nullspam.ru/");
                case "HUSH":
                    return new InsightInfoProvider("https://explorer.myhush.org/api");
                case "TZC":
                    return new IquidusInfoProvider("http://tzc.explorerz.top:3004/");
                default:
                    return new DummyInfoProvider();
            }
        }

        private class DummyInfoProvider : ICoinNetworkInfoProvider
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
