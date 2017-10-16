using System;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Profitability
{
    public class ProfitabilityCalculator : IProfitabilityCalculator
    {
        private const int SecondsInDay = 60 * 60 * 24;
        private static readonly double M_BtcLikeCoefficient = Math.Pow(2, 48) / 0xFFFF;
        private static readonly double M_M7Coefficient = Math.Pow(2, 20);

        public double CalculateCoinsPerDay(Coin coin, CoinNetworkInfo networkInfo, long yourHashRate)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));
            if (networkInfo == null)
                throw new ArgumentNullException(nameof(networkInfo));

            if (networkInfo.Difficulty <= 0 && networkInfo.NetHashRate <= 0)
                return 0;
            switch (coin.Algorithm.ProfitabilityFormulaType)
            {
                case ProfitabilityFormulaType.BitcoinLike:
                    return CalculateByDifficulty(yourHashRate, networkInfo.BlockReward, networkInfo.Difficulty, M_BtcLikeCoefficient);
                case ProfitabilityFormulaType.ByHashRate:
                    return CalculateByNetHashRate(
                        yourHashRate, networkInfo.NetHashRate, networkInfo.BlockReward, networkInfo.BlockTimeSeconds);
                case ProfitabilityFormulaType.EtHash:
                    return CalculateByNetHashRate(
                        yourHashRate, (long)(networkInfo.Difficulty / networkInfo.BlockTimeSeconds), 
                        networkInfo.BlockReward, networkInfo.BlockTimeSeconds);
                case ProfitabilityFormulaType.Special:
                    return CalculateSpecial(coin, networkInfo, yourHashRate);
                default:
                    return 0;
            }
        }

        private static double CalculateSpecial(Coin coin, CoinNetworkInfo network, long hashRate)
        {
            switch (coin.Algorithm.KnownValue)
            {
                case KnownCoinAlgorithm.PrimeChain:
                    //For PrimeChain units, 1 Scrypt Mh = 15.6 CPD, as http://xpmforall.org/ says
                    hashRate = (long)(hashRate / 15.6 * 1e6);
                    return CalculateByDifficulty(hashRate, network.BlockReward, network.Difficulty, M_BtcLikeCoefficient);
                case KnownCoinAlgorithm.Equihash:
                    return CalculateForEquihash(coin, network, hashRate);
                case KnownCoinAlgorithm.M7:
                    return CalculateByDifficulty(hashRate, network.BlockReward, network.Difficulty, M_M7Coefficient);
                case KnownCoinAlgorithm.Blake2B:
                    return CalculateByDifficulty(hashRate, network.BlockReward, network.Difficulty, 1);
                default:
                    return 0;
            }
        }

        private static double CalculateForEquihash(Coin coin, CoinNetworkInfo networkInfo, long yourHashRate) 
            => SecondsInDay * networkInfo.BlockReward / (networkInfo.Difficulty * coin.SolsPerDiff.GetValueOrDefault() / yourHashRate);

        private static double CalculateByNetHashRate(
            double yourHashRate, double netHashRate, double blockReward, double blockTimeSec)
            => SecondsInDay * yourHashRate / (yourHashRate + netHashRate) * (blockReward / blockTimeSec);

        private static double CalculateByDifficulty(long yourHashRate, double blockReward, double difficulty, double coefficient)
            => SecondsInDay * blockReward * yourHashRate / (difficulty * coefficient);
    }
}
