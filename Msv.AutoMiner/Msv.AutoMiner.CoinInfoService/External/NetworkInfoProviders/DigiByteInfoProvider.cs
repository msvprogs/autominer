using System;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    //Block chain explorer: https://digiexplorer.info
    public class DigiByteInfoProvider : InsightInfoProvider
    {
        private readonly KnownCoinAlgorithm m_Algorithm;

        public DigiByteInfoProvider(IWebClient webClient, KnownCoinAlgorithm algorithm)
            : base(webClient, "https://digiexplorer.info/api")
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
