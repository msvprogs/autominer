namespace Msv.AutoMiner.CoinInfoService.Logic.Profitability
{
    public interface IBlockRewardCalculator
    {
        double? Calculate(string code, long height, double? difficulty, double? moneySupply, int? masternodeCount);
    }
}
