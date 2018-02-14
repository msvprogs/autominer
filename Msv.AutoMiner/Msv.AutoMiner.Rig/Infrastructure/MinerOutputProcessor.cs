using System;
using System.Collections.Generic;
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
        private readonly bool m_BenchmarkMode;

        private readonly Regex m_SpeedRegex;
        private readonly Regex m_BenchmarkSpeedRegex;
        private readonly Regex m_ValidShareRegex;
        private readonly Regex m_InvalidShareRegex;

        public double CurrentHashRate => m_IndividualHashrates.Select(x => x.Value).DefaultIfEmpty(0).Sum();
        public int AcceptedShares { get; private set; }
        public int RejectedShares { get; private set; }

        private readonly Dictionary<string, double> m_IndividualHashrates =
            new Dictionary<string, double>();

        public MinerOutputProcessor(Miner miner, bool benchmarkMode)
        {
            m_Miner = miner ?? throw new ArgumentNullException(nameof(miner));
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
                    m_IndividualHashrates[string.Empty] = ParsingHelper.ParseHashRate(benchmarkMatch.Groups["speed"].Value);
                    return;
                }
            }

            m_SpeedRegex?.Matches(output)
                .Cast<Match>()
                .Select(x => new
                {
                    Gpu = x.Groups["gpu"].Value,
                    Speed = x.Groups["speed"].Value
                })
                .GroupBy(x => x.Gpu)
                .ForEach(x => m_IndividualHashrates[x.Key] = ParsingHelper.ParseHashRate(x.Last().Speed));
        }

        public void Dispose()
        { }
    }
}
