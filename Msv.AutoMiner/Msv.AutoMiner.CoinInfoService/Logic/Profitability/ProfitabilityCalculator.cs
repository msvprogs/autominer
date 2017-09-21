using System;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Profitability
{
    public class ProfitabilityCalculator : IProfitabilityCalculator
    {
        private const int SecondsInDay = 60 * 60 * 24;
        private static readonly double M_DifficultyMultiplier = Math.Pow(2, 48) / 0xFFFF;

        public double CalculateCoinsPerDay(Coin coin, CoinNetworkInfo networkInfo, long yourHashRate)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));
            if (networkInfo == null)
                throw new ArgumentNullException(nameof(networkInfo));

            if (networkInfo.Difficulty <= 0 && networkInfo.NetHashRate == 0)
                return 0;
            switch (coin.Algorithm.ProfitabilityFormulaType)
            {
                case ProfitabilityFormulaType.BitcoinLike:
                    return CalculateByDifficulty(yourHashRate, networkInfo.BlockReward, networkInfo.Difficulty);
                case ProfitabilityFormulaType.ByHashRate:
                    return CalculateByNetHashRate(
                        yourHashRate, networkInfo.NetHashRate, networkInfo.BlockReward, networkInfo.BlockTimeSeconds);
                case ProfitabilityFormulaType.Equihash:
                    return CalculateForEquihash(coin, networkInfo, yourHashRate);
                case ProfitabilityFormulaType.EtHash:
                    return CalculateByNetHashRate(
                        yourHashRate, (long)(networkInfo.Difficulty / networkInfo.BlockTimeSeconds), 
                        networkInfo.BlockReward, networkInfo.BlockTimeSeconds);
                case ProfitabilityFormulaType.PrimeChain:
                    //For PrimeChain units, 1 Scrypt Mh = 15.6 CPD, as http://xpmforall.org/ says
                    yourHashRate = (long)(yourHashRate / 15.6 * 1e6);
                    goto case ProfitabilityFormulaType.BitcoinLike;
                default:
                    return 0;
            }
        }

        private static double CalculateForEquihash(Coin coin, CoinNetworkInfo networkInfo, long yourHashRate) 
            => SecondsInDay * networkInfo.BlockReward / (networkInfo.Difficulty * coin.SolsPerDiff.GetValueOrDefault() / yourHashRate);

        private static double CalculateByNetHashRate(
            long yourHashRate, long netHashRate, double blockReward, double blockTimeSec)
            => SecondsInDay * (double)yourHashRate / (yourHashRate + netHashRate) * (blockReward / blockTimeSec);

        private static double CalculateByDifficulty(long yourHashRate, double blockReward, double difficulty)
            => SecondsInDay * blockReward * yourHashRate / (difficulty * M_DifficultyMultiplier);
    }
}
