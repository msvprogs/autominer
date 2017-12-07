using System;
using Jint;
using NLog;

namespace Msv.AutoMiner.CoinInfoService.Logic.Profitability
{
    public class JsBlockRewardCalculator : IBlockRewardCalculator
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        public double? Calculate(string code, long height)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            try
            {
                return new Engine(x => x.TimeoutInterval(TimeSpan.FromSeconds(2)))
                    .Execute($"function calc(height) {{ {code} }} calc({height});")
                    .GetCompletionValue()
                    .AsNumber();
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex, "JS execution exception in code " + code);
                return null;
            }
        }
    }
}
