using System;
using System.Linq;
using System.Text.RegularExpressions;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class MinerOutputProcessor : IMinerOutputProcessor
    {
        private readonly string m_ProcessName;
        private readonly string m_PrimaryCurrency;
        private readonly string m_SecondaryCurrency;
        private static readonly ILogger M_MinerOutputLogger = LogManager.GetLogger("MinerOutput");

        private readonly Regex m_SpeedRegex;
        private readonly Regex m_ValidShareRegex;

        public long CurrentHashRate { get; private set; }
        public long CurrentSecondaryHashRate { get; private set; }
        public int? AcceptedShares { get; private set; }

        public MinerOutputProcessor(string processName, string speedRegex, string validShareRegex,
            string primaryCurrency, string secondaryCurrency)
        {
            if (string.IsNullOrEmpty(primaryCurrency))
                throw new ArgumentException("Value cannot be null or empty.", nameof(primaryCurrency));

            m_ProcessName = processName ?? throw new ArgumentNullException(nameof(processName));
            m_PrimaryCurrency = primaryCurrency;
            m_SecondaryCurrency = secondaryCurrency;

            if (!string.IsNullOrWhiteSpace(speedRegex))
                m_SpeedRegex = new Regex(speedRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (string.IsNullOrWhiteSpace(validShareRegex))
                return;
            m_ValidShareRegex = new Regex(validShareRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            AcceptedShares = 0;
        }

        public void Write(string output)
        {
            M_MinerOutputLogger.Debug($"{m_ProcessName}: {output}");
            if (string.IsNullOrWhiteSpace(output))
                return;
            if (m_ValidShareRegex != null)
                AcceptedShares += m_ValidShareRegex.Matches(output).Count;

            if (m_SpeedRegex == null)
                return;
            var matches = m_SpeedRegex.Matches(output)
                .Cast<Match>()
                .Select(x => new
                {
                    Speed = x.Groups["speed"].Value,
                    Currency = x.Groups["currency"].Value.ToUpperInvariant()
                })
                .GroupBy(x => x.Currency)
                .ToDictionary(x => x.Key, x => x.Last().Speed);
            if (!matches.Any())
                return;
            var primaryHashRate = matches.TryGetValue(m_PrimaryCurrency)
                                  ?? matches.TryGetValue("ETH") // Claymore states primary currency as ETH, even if it is a fork
                                  ?? matches.TryGetValue(string.Empty);
            if (!string.IsNullOrEmpty(primaryHashRate))
                CurrentHashRate = ParsingHelper.ParseHashRate(primaryHashRate);
            if (string.IsNullOrEmpty(m_SecondaryCurrency))
                return;
            var secondaryHashRate = matches.TryGetValue(m_SecondaryCurrency);
            if (!string.IsNullOrEmpty(secondaryHashRate))
                CurrentSecondaryHashRate = ParsingHelper.ParseHashRate(secondaryHashRate);
        }
    }
}
