using System;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Common;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    public class MyriadCoinInfoProvider : InsightInfoProvider
    {
        private readonly KnownCoinAlgorithm m_Algorithm;

        public MyriadCoinInfoProvider(IWebClient webClient, KnownCoinAlgorithm algorithm)
            : base(webClient, "https://cryptap.us/myr/insight/api")
        {
            m_Algorithm = algorithm;
        }

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
    }
}
