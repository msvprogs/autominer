using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class MaxCoinInfoProvider : SwaggerInfoProviderBase
    {
        private const int BlocksPerDay = 60 * 24;
        private const int BlocksPerYear = BlocksPerDay * 365;
        private const int BlocksPerHalvingPeriod = BlocksPerYear * 4;

        public MaxCoinInfoProvider(IWebClient webClient) 
            : base(webClient, "http://api.maxcoinhub.io")
        { }

        protected override double GetBlockReward(long height)
        {
            if (height == 0)
                return 5;

            int reward;
            if (height > 600000)
                reward = 16;
            else if (height > 140000)
                reward = 48;
            else
                reward = 96;

            reward >>= (int)(height / BlocksPerHalvingPeriod);
            return reward;
        }
    }
}