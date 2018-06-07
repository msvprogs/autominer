using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Common.Notifiers;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Logic.Analyzers
{
    public class HeartbeatAnalyzer : IHeartbeatAnalyzer
    {
        private readonly INotifier m_Notifier;
        private readonly HeartbeatAnalyzerParams m_Options;
        private readonly ConcurrentDictionary<int, RigState> m_RigStates = new ConcurrentDictionary<int, RigState>();

        public HeartbeatAnalyzer(INotifier notifier, HeartbeatAnalyzerParams options)
        {
            m_Notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            m_Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Analyze(Rig rig, Heartbeat heartbeat)
        {
            var state = m_RigStates.GetOrAdd(rig.Id, new RigState());
            var videoStates = heartbeat.VideoAdapterStates.EmptyIfNull();
            if (videoStates.Any())
            {
                if (videoStates.Min(x => x.Utilization) < m_Options.MinVideoUsage)
                    state.LowVideoUsages.Add(videoStates.Min(x => x.Utilization));
                else
                    state.LowVideoUsages.Clear();

                if (videoStates.Max(x => x.Temperature.Current) > m_Options.MaxVideoTemperature)
                    state.HighVideoTemperatures.Add(videoStates.Max(x => x.Temperature.Current));
                else
                    state.HighVideoTemperatures.Clear();
            }
            var miningStates = heartbeat.MiningStates.EmptyIfNull();
            if (miningStates.Any())
            {
                var maxInvalidShareRate = miningStates
                    .Where(x => x.ValidShares > 0 || x.InvalidShares > 0)
                    .Select(x => (double) x.InvalidShares / (x.ValidShares + x.InvalidShares) * 100)
                    .DefaultIfEmpty(0)
                    .Max();
                if (maxInvalidShareRate > m_Options.MaxInvalidSharesRate)
                    state.InvalidShareRates.Add((int) maxInvalidShareRate);
                else
                    state.InvalidShareRates.Clear();

                var maxUnusualHashrateDiff = miningStates
                    .Where(x => x.HashRate.Current > 0)
                    .Select(x => ConversionHelper.GetDiffRatio(x.HashRate.Reference, x.HashRate.Current))
                    .Select(Math.Abs)
                    .DefaultIfEmpty(0)
                    .Max();
                if (maxUnusualHashrateDiff > m_Options.MaxHashrateDifference)
                    state.UnusualHashrateDifferences.Add((int) maxUnusualHashrateDiff);
                else
                    state.UnusualHashrateDifferences.Clear();
            }
            CheckStateAndNotify(rig, state);
        }

        private void CheckStateAndNotify(Rig rig, RigState state)
        {
            if (state.LowVideoUsages.Count >= m_Options.SamplesCount)
            {
                m_Notifier.SendMessage(
                    CreateMessage(rig, "Some video adapters have very low usage", state.LowVideoUsages.ToArray(), "%"));
                state.LowVideoUsages.Clear();
            }
            if (state.HighVideoTemperatures.Count >= m_Options.SamplesCount)
            {
                m_Notifier.SendMessage(
                    CreateMessage(rig, "Some video adapters are overheated", state.HighVideoTemperatures.ToArray(), "°C"));
                state.HighVideoTemperatures.Clear();
            }
            if (state.InvalidShareRates.Count >= m_Options.SamplesCount)
            {
                m_Notifier.SendMessage(
                    CreateMessage(rig, "There are too many invalid shares", state.InvalidShareRates.ToArray(), "%"));
                state.InvalidShareRates.Clear();
            }
            if (state.UnusualHashrateDifferences.Count >= m_Options.SamplesCount)
            {
                m_Notifier.SendMessage(CreateMessage(rig, "Current hashrate differs too much from reference one",
                    state.UnusualHashrateDifferences.ToArray(), "%"));
                state.UnusualHashrateDifferences.Clear();
            }
        }

        private static string CreateMessage(Rig rig, string problem, IReadOnlyCollection<int> values, string valuePostfix)
        {
            //language=html
            const string messageFormat = @"<b>Warning!</b>
Your rig '{0}' is experiencing the following problem:
<i>{1}</i>
Last {2} measured values: {3}";
            return string.Format(messageFormat, rig.Name, problem, values.Count, string.Join(", ", values.Select(x => x + valuePostfix)));
        }

        private class RigState
        {
            public List<int> LowVideoUsages { get; } = new List<int>();
            public List<int> HighVideoTemperatures { get; } = new List<int>();
            public List<int> InvalidShareRates { get; } = new List<int>();
            public List<int> UnusualHashrateDifferences { get; } = new List<int>();
        }
    }
}
