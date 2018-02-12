﻿using System;
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
        private static readonly ILogger M_MinerOutputLogger = LogManager.GetLogger("MinerOutput");

        private readonly Miner m_Miner;
        private readonly string m_PrimaryCurrency;
        private readonly string m_SecondaryCurrency;
        private readonly bool m_BenchmarkMode;

        private readonly Regex m_SpeedRegex;
        private readonly Regex m_BenchmarkSpeedRegex;
        private readonly Regex m_ValidShareRegex;
        private readonly Regex m_InvalidShareRegex;

        public long CurrentHashRate { get; private set; }
        public long CurrentSecondaryHashRate { get; private set; }
        public int AcceptedShares { get; private set; }
        public int RejectedShares { get; private set; }

        public MinerOutputProcessor(Miner miner, string primaryCurrency, string secondaryCurrency, bool benchmarkMode)
        {
            m_Miner = miner ?? throw new ArgumentNullException(nameof(miner));
            m_PrimaryCurrency = primaryCurrency;
            m_SecondaryCurrency = secondaryCurrency;
            m_BenchmarkMode = benchmarkMode;

            if (!string.IsNullOrWhiteSpace(miner.SpeedRegex))
                m_SpeedRegex = new Regex(miner.SpeedRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!string.IsNullOrWhiteSpace(miner.BenchmarkResultRegex))
                m_BenchmarkSpeedRegex = new Regex(miner.BenchmarkResultRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!string.IsNullOrWhiteSpace(miner.ValidShareRegex))
                m_ValidShareRegex = new Regex(miner.ValidShareRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (!string.IsNullOrWhiteSpace(miner.InvalidShareRegex))
                m_InvalidShareRegex = new Regex(miner.InvalidShareRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        public void Write(string output)
        {
            M_MinerOutputLogger.Debug($"{m_Miner.Name}: {output}");
            if (string.IsNullOrWhiteSpace(output))
                return;
            if (m_ValidShareRegex != null)
                AcceptedShares += m_ValidShareRegex.Matches(output).Count;
            if (m_InvalidShareRegex != null)
                RejectedShares += m_InvalidShareRegex.Matches(output).Count;
            if (m_BenchmarkMode && m_BenchmarkSpeedRegex != null)
            {
                var benchmarkMatch = m_BenchmarkSpeedRegex.Match(output);
                if (benchmarkMatch.Success)
                {
                    CurrentHashRate = (long)ParsingHelper.ParseHashRate(benchmarkMatch.Groups["speed"].Value);
                    return;
                }
            }

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
                                  ?? matches.TryGetValue(string.Empty);
            if (!string.IsNullOrEmpty(primaryHashRate))
                CurrentHashRate = (long)ParsingHelper.ParseHashRate(primaryHashRate);
            if (string.IsNullOrEmpty(m_SecondaryCurrency))
                return;
            var secondaryHashRate = matches.TryGetValue(m_SecondaryCurrency);
            if (!string.IsNullOrEmpty(secondaryHashRate))
                CurrentSecondaryHashRate = (long)ParsingHelper.ParseHashRate(secondaryHashRate);
        }

        public void Dispose()
        { }
    }
}
