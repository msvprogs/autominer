using System;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Infrastructure.Contracts;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class ProfitabilityCalculator : IProfitabilityCalculator
    {
        private const int SecondsInDay = 60 * 60 * 24;
        private static readonly double M_DifficultyMultiplier = Math.Pow(2, 48) / 0xFFFF;

        public double CalculateCoinsPerDay(Coin coin, long yourHashRate)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));
            if (yourHashRate < 0)
                throw new ArgumentOutOfRangeException(nameof(yourHashRate));

            if (coin.Difficulty <= 0 && coin.NetHashRate == 0)
                return 0;
            if (coin.Algorithm == CoinAlgorithm.Equihash
                && coin.Difficulty > 0
                && coin.SolsPerDiff != null)
                return SecondsInDay * coin.BlockReward / (coin.Difficulty * coin.SolsPerDiff.Value / yourHashRate);
            //For PrimeChain units, 1 Scrypt Mh = 15.6 CPD, as http://xpmforall.org/ says
            if (coin.Algorithm == CoinAlgorithm.PrimeChain)
                yourHashRate = (long)(yourHashRate / 15.6 * 1e6);
            if (coin.Algorithm != CoinAlgorithm.Equihash
                && coin.Algorithm != CoinAlgorithm.EtHash
                && coin.Algorithm != CoinAlgorithm.CryptoNight
                && coin.Algorithm != CoinAlgorithm.Blake2B
                && coin.Algorithm != CoinAlgorithm.Pascal
                && coin.Algorithm != CoinAlgorithm.M7)
                return CalculateByDifficulty(yourHashRate, coin.BlockReward, coin.Difficulty);

            var netHashRate = coin.NetHashRate;
            if (coin.Algorithm == CoinAlgorithm.EtHash)
                netHashRate = (long) (coin.Difficulty / coin.BlockTimeSeconds);
            return CalculateByNetHashRate(yourHashRate, netHashRate, coin.BlockReward, coin.BlockTimeSeconds);
        }

        private static double CalculateByNetHashRate(
            long yourHashRate, long netHashRate, double blockReward, double blockTimeSec)
            => SecondsInDay * (double)yourHashRate / (yourHashRate + netHashRate) * (blockReward / blockTimeSec);

        private static double CalculateByDifficulty(long yourHashRate, double blockReward, double difficulty)
            => SecondsInDay * blockReward * yourHashRate / (difficulty * M_DifficultyMultiplier);
    }
}
