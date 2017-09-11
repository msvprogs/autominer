using System;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External.Network
{
    //Block chain explorer: https://digiexplorer.info
    public class DigiByteInfoProvider : InsightInfoProvider
    {
        private readonly CoinAlgorithm m_Algorithm;

        public DigiByteInfoProvider(CoinAlgorithm algorithm)
            : base("https://digiexplorer.info/api")
        {
            m_Algorithm = algorithm;
        }

        protected override bool IsUsableBlock(dynamic blockInfo) 
            => (string) blockInfo.algo == GetAlgorithmString();

        protected override double GetDifficulty(dynamic statsInfo)
        {
            switch (m_Algorithm)
            {
                case CoinAlgorithm.MyriadGroestl:
                    return (double)statsInfo.difficulty_groestl;
                case CoinAlgorithm.Skein:
                    return (double)statsInfo.difficulty_skein;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string GetAlgorithmString()
        {
            switch (m_Algorithm)
            {
                case CoinAlgorithm.MyriadGroestl:
                    return "groestl";
                case CoinAlgorithm.Skein:
                    return "skein";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
