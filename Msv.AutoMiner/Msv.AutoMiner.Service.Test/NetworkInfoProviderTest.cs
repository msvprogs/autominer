using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.External;
using Msv.AutoMiner.Service.External.Exchanges;
using Msv.AutoMiner.Service.External.Network.Common;
using Msv.AutoMiner.Service.External.Pools;
using Msv.AutoMiner.Service.Infrastructure;
using Msv.AutoMiner.Service.Security;
using Msv.AutoMiner.Service.Storage;
using Msv.AutoMiner.Service.Video.NVidia;
using Msv.AutoMiner.Service.Video.NVidia.Nvml;
using NUnit.Framework;

namespace Msv.AutoMiner.Service.Test
{
    [TestFixture]
    public class NetworkInfoProviderTest
    {
        [Test]
        public void Test()
        {
            //var prov = new XpmForAllPoolInfoProvider(
            //    "http://xpmforall.org",
            //    "Adsez6jqVWzJfC7mDPMPssvdmy8aEX5rrf");
            //var data = prov.GetInfo(DateTime.MaxValue);

            //var prov = new NovaexchangeMarketInfoProvider();
            //var res = prov.GetCoinMarketInfos(new[] { "SWEEP", "DNR" });

            //var marketinfoprov = new BitzureMarketInfoProvider();
            //var mrk = marketinfoprov.GetCoinMarketInfos(new string[0]);


            var factory = new CoinNetworkInfoProviderFactory();
            var result = factory.Create(new Coin { CurrencySymbol = "TZC", }).GetNetworkStats();

            //var result = factory.CreateMulti(new[]
            //    {new Coin {CurrencySymbol = "KMD", Algorithm = CoinAlgorithm.Blake2S}}, new DDoSTriggerPreventingDownloader()).GetMultiNetworkStats();

            Console.WriteLine("Difficulty: " + result.Difficulty);
            Console.WriteLine("HashRate: " + result.NetHashRate);
            Console.WriteLine("Reward: " + result.BlockReward);
            Console.WriteLine("BlockTime: " + result.BlockTimeSeconds);
            Console.WriteLine("Height: " + result.Height);

            //var factory = new PoolInfoProviderFactory();
            //var result = factory.Create(new Coin
            //{
            //    Pool = new Pool
            //    {
            //        ApiProtocol = PoolApiProtocol.Yiimp,
            //        ApiUrl = "http://lpool.name/api",
            //        ApiPoolName = "skein",
            //        IsAnonymous = true,
            //    },
            //    Wallet = "bJnHQAPkcWPWBUN5r47sgZpf3nVGybDFcQ"
            //}, new DDoSTriggerPreventingDownloader()).GetInfo(DateTime.MinValue);

            //Console.WriteLine(result.ConfirmedBalance);
            //Console.WriteLine(result.Hashrate);
            //Console.WriteLine(result.UnconfirmedBalance);
            //Console.WriteLine(result.ValidShares);
        }
    }
}
