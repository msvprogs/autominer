using System;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Profitability
{
    public class ProfitabilityCalculator : IProfitabilityCalculator
    {
        private const int SecondsInDay = 60 * 60 * 24;
        private static readonly double M_32ByteHashesCount = Math.Pow(256, 32);
        private static readonly double M_BtcMaxTarget =
            (double) HexHelper.HexToBigInteger("0x00000000FFFF0000000000000000000000000000000000000000000000000000");

        public double CalculateCoinsPerDay(Coin coin, CoinNetworkInfo networkInfo, double yourHashRate)
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
                    var maxTarget = string.IsNullOrEmpty(coin.MaxTarget)
                        ? M_BtcMaxTarget
                        : (double) HexHelper.HexToBigInteger(coin.MaxTarget);
                    return CalculateByDifficulty(yourHashRate, networkInfo.BlockReward, networkInfo.Difficulty, maxTarget);
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

        private static double CalculateSpecial(Coin coin, CoinNetworkInfo network, double hashRate)
        {
            switch (coin.Algorithm.KnownValue)
            {
                case KnownCoinAlgorithm.PrimeChain:
                    //For PrimeChain units, 1 Scrypt Mh = 15.6 CPD, as http://xpmforall.org/ says
                    hashRate = (long)(hashRate / 15.6 * 1e6);
                    return CalculateByDifficulty(hashRate, network.BlockReward, network.Difficulty, M_BtcMaxTarget);
                case KnownCoinAlgorithm.Blake2B:
                    return CalculateByDifficulty(hashRate, network.BlockReward, network.Difficulty, 1);
                default:
                    return 0;
            }
        }

        private static double CalculateByNetHashRate(
            double yourHashRate, double netHashRate, double blockReward, double blockTimeSec)
            => SecondsInDay * yourHashRate / (yourHashRate + netHashRate) * (blockReward / blockTimeSec);

        private static double CalculateByDifficulty(double yourHashRate, double blockReward, double difficulty, double maxTarget)
            => SecondsInDay * blockReward * yourHashRate * maxTarget / (difficulty * M_32ByteHashesCount);
    }
}
