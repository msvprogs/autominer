using System;
using System.Globalization;
using Jint;
using NLog;

namespace Msv.AutoMiner.CoinInfoService.Logic.Profitability
{
    public class JsBlockRewardCalculator : IBlockRewardCalculator
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        public double? Calculate(string code, long height, double? difficulty, double? moneySupply, int? masternodeCount)
        {
            if (code == null)
                throw new ArgumentNullException(nameof(code));

            try
            {
                return new Engine(x => x.TimeoutInterval(TimeSpan.FromSeconds(2)))
                    .Execute($@"
function halve(value, times) {{
    return value / Math.pow(2, times ^ 0);
}}

function calc(height, difficulty, moneySupply, masternodeCount) 
{{ 
{code}
}}

calc({height}, {NullableToString(difficulty)}, {NullableToString(moneySupply)}, {NullableToString(masternodeCount)});")
                    .GetCompletionValue()
                    .AsNumber();
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex, "JS execution exception in code " + code);
                return null;
            }

            string NullableToString<T>(T? value)
                where T : struct
                => value != null ? ((IFormattable)value.Value).ToString("G", CultureInfo.InvariantCulture) : "null";
        }
    }
}
