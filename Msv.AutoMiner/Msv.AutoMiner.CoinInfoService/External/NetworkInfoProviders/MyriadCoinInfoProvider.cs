using System;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class MyriadCoinInfoProvider : InsightInfoProvider
    {
        private readonly KnownCoinAlgorithm m_Algorithm;

        public MyriadCoinInfoProvider(IWebClient webClient, KnownCoinAlgorithm algorithm)
            : base(webClient, "https://cryptap.us/myr/insight/api")
        {
            m_Algorithm = algorithm;
        }

        protected override bool IsUsableBlock(dynamic blockInfo)
            => (string)blockInfo.pow_algo == GetAlgorithmString();

        protected override double GetDifficulty(dynamic statsInfo)
        {
            switch (m_Algorithm)
            {
                case KnownCoinAlgorithm.MyriadGroestl:
                    return (double)statsInfo.difficulty_groestl;
                case KnownCoinAlgorithm.Skein:
                    return (double)statsInfo.difficulty_skein;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statsInfo));
            }
        }

        private string GetAlgorithmString()
        {
            switch (m_Algorithm)
            {
                case KnownCoinAlgorithm.MyriadGroestl:
                    return "groestl";
                case KnownCoinAlgorithm.Skein:
                    return "skein";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
