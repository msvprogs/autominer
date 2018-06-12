using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    [SpecificCoinInfoProvider("MAX")]
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

            double reward;
            if (height > 600000)
                reward = 16;
            else if (height > 140000)
                reward = 48;
            else
                reward = 96;

            reward /= Math.Pow(2, (int)(height / BlocksPerHalvingPeriod));
            return reward;
        }

        public override WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }
    }
}