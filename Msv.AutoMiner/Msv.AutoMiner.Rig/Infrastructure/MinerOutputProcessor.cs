using System;
using System.Linq;
using System.Text.RegularExpressions;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;
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
        private readonly Regex m_InvalidShareRegex;

        public long CurrentHashRate { get; private set; }
        public long CurrentSecondaryHashRate { get; private set; }
        public int? AcceptedShares { get; private set; }
        public int? RejectedShares { get; private set; }

        public MinerOutputProcessor(string processName, Miner miner, string primaryCurrency, string secondaryCurrency)
        {
            if (miner == null)
                throw new ArgumentNullException(nameof(miner));
            if (string.IsNullOrEmpty(primaryCurrency))
                throw new ArgumentException("Value cannot be null or empty.", nameof(primaryCurrency));

            m_ProcessName = processName ?? throw new ArgumentNullException(nameof(processName));
            m_PrimaryCurrency = primaryCurrency;
            m_SecondaryCurrency = secondaryCurrency;

            if (!string.IsNullOrWhiteSpace(miner.SpeedRegex))
                m_SpeedRegex = new Regex(miner.SpeedRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!string.IsNullOrWhiteSpace(miner.ValidShareRegex))
                m_ValidShareRegex = new Regex(miner.ValidShareRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!string.IsNullOrWhiteSpace(miner.InvalidShareRegex))
                m_InvalidShareRegex = new Regex(miner.InvalidShareRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            AcceptedShares = RejectedShares = 0;
        }

        public void Write(string output)
        {
            M_MinerOutputLogger.Debug($"{m_ProcessName}: {output}");
            if (string.IsNullOrWhiteSpace(output))
                return;
            if (m_ValidShareRegex != null)
                AcceptedShares += m_ValidShareRegex.Matches(output).Count;
            if (m_InvalidShareRegex != null)
                RejectedShares += m_InvalidShareRegex.Matches(output).Count;

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
