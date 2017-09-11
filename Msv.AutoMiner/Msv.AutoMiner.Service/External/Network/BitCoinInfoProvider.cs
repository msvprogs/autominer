using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.External.Network.Common;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    //API: https://blockexplorer.com/api-ref
    //API2: https://blockchain.info/api/blockchain_api
    //API3: https://blockchain.info/api/charts_api
    public class BitCoinInfoProvider : NetworkInfoProviderBase, IBitCoinMarketPriceProvider
    {
        public override CoinNetworkStatistics GetNetworkStats()
        {
            var blockRewardString = DownloadString("https://blockchain.info/ru/q/bcperblock");
            var statsJson = GetCurrentBitCoinStats();
            return new CoinNetworkStatistics
            {
                Difficulty = (double)statsJson.difficulty,
                BlockReward = double.Parse(blockRewardString) / 1e8,
                BlockTimeSeconds = (double)statsJson.minutes_between_blocks * 60,
                NetHashRate = (long)((double)statsJson.hash_rate * 1e9),
                Height = (long)statsJson.n_blocks_total
            };
        }

        public double GetCurrentPriceUsd() 
            => (double) GetCurrentBitCoinStats().market_price_usd;

        private dynamic GetCurrentBitCoinStats()
            => JsonConvert.DeserializeObject(DownloadString("https://api.blockchain.info/stats"));
    }
}
