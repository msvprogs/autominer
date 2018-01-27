using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.ControlCenterService.Logic.Notifiers;

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

        public void Analyze(int rigId, Heartbeat heartbeat)
        {
            var state = m_RigStates.GetOrAdd(rigId, new RigState());
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
                    .Select(x => ((double) x.HashRate.Reference - x.HashRate.Current) / x.HashRate.Current * 100)
                    .Select(Math.Abs)
                    .DefaultIfEmpty(0)
                    .Max();
                if (maxUnusualHashrateDiff > m_Options.MaxHashrateDifference)
                    state.UnusualHashrateDifferences.Add((int) maxUnusualHashrateDiff);
                else
                    state.UnusualHashrateDifferences.Clear();
            }
            CheckStateAndNotify(rigId, state);
        }

        private void CheckStateAndNotify(int rigId, RigState state)
        {
            if (state.LowVideoUsages.Count >= m_Options.SamplesCount)
            {
                m_Notifier.NotifyLowVideoUsage(rigId, state.LowVideoUsages.ToArray());
                state.LowVideoUsages.Clear();
            }
            if (state.HighVideoTemperatures.Count >= m_Options.SamplesCount)
            {
                m_Notifier.NotifyHighVideoTemperature(rigId, state.HighVideoTemperatures.ToArray());
                state.HighVideoTemperatures.Clear();
            }
            if (state.InvalidShareRates.Count >= m_Options.SamplesCount)
            {
                m_Notifier.NotifyHighInvalidShareRate(rigId, state.InvalidShareRates.ToArray());
                state.InvalidShareRates.Clear();
            }
            if (state.UnusualHashrateDifferences.Count >= m_Options.SamplesCount)
            {
                m_Notifier.NotifyUnusualHashRate(rigId, state.UnusualHashrateDifferences.ToArray());
                state.UnusualHashrateDifferences.Clear();
            }
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
