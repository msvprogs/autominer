using System;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    public class PrimeCoinInfoProvider : WebDownloaderBase, ICoinNetworkInfoProvider
    {
        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(DownloadString(
                "http://xpm.muuttuja.org/calc/current_block.json"));
            var difficulty = (double) json.difficulty;
            return new CoinNetworkStatistics
            {
                Difficulty = difficulty,
                BlockReward = Math.Floor(99900 / (difficulty * difficulty)) / 100,
                Height = (long) json.height
            };
        }
    }
}
