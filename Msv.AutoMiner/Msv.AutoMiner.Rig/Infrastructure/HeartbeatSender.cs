using System;
using System.Linq;
using System.Reflection;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Remote;
using Msv.AutoMiner.Rig.System.Video;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class HeartbeatSender : MonitorBase
    {
        private static readonly Version M_AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;

        private readonly IVideoAdapterMonitor m_VideoAdapterMonitor;
        private readonly IMinerProcessController m_MinerProcessController;
        private readonly IControlCenterService m_Service;

        public HeartbeatSender(
            IVideoAdapterMonitor videoAdapterMonitor,
            IMinerProcessController minerProcessController,
            IControlCenterService service)
            : base(TimeSpan.FromMinutes(1), true)
        {
            m_VideoAdapterMonitor = videoAdapterMonitor ?? throw new ArgumentNullException(nameof(videoAdapterMonitor));
            m_MinerProcessController =
                minerProcessController ?? throw new ArgumentNullException(nameof(minerProcessController));
            m_Service = service ?? throw new ArgumentNullException(nameof(service));
        }

        protected override void DoWork()
        {
            var currentState = m_MinerProcessController.CurrentState;
            var heartbeatMiningState = currentState != null
                ? new Heartbeat.MiningState
                {
                    CoinId = currentState.CoinId,
                    PoolId = currentState.PoolId,
                    Duration = DateTime.UtcNow - m_MinerProcessController.StateChanged,
                    ValidShares = currentState.AcceptedShares.GetValueOrDefault(),
                    InvalidShares = currentState.RejectedShares.GetValueOrDefault(),
                    HashRate = new Heartbeat.ValueWithReference<long>
                    {
                        Current = currentState.CurrentHashRate,
                        Reference = currentState.StoredHashRate
                    }
                }
                : null;
            var videoState = m_VideoAdapterMonitor.GetCurrentState();
            var heartbeat = new Heartbeat
            {
                DateTime = DateTime.UtcNow,
                ClientVersion = M_AssemblyVersion.ToString(),
                MiningStates = heartbeatMiningState != null ? new []{ heartbeatMiningState } : null,
                OsVersion = Environment.OSVersion.ToString(),
                VideoDriverVersion = videoState?.DriverVersion,
                VideoAdapterStates = videoState?.AdapterStates
                    .Select(ToHeartbeatVideoAdapterState)
                    .ToArray()
            };
            m_Service.SendHeartbeat(heartbeat);
        }

        private static Heartbeat.VideoAdapterState ToHeartbeatVideoAdapterState(VideoAdapterState state)
            => new Heartbeat.VideoAdapterState
            {
                Name = state.Name,
                BiosVersion = state.VbiosVersion,
                CoreClockMhz = new Heartbeat.ValueWithLimits<int>
                {
                    Current = (int) state.GpuClocksMhz,
                    Max = (int) state.GpuMaxClocksMhz
                },
                FanSpeed = (int) state.FanSpeed,
                MemoryClockMhz = new Heartbeat.ValueWithLimits<int>
                {
                    Current = (int) state.MemoryClocksMhz,
                    Max = (int) state.MemoryMaxClocksMhz
                },
                MemoryUsageMb = new Heartbeat.ValueWithLimits<int>
                {
                    Current = (int) state.UsedMemoryMb,
                    Max = (int) state.TotalMemoryMb
                },
                PerformanceState = state.PerformanceState.ToString(),
                PowerUsageWatts = new Heartbeat.ValueWithLimits<double>
                {
                    Current = (double) state.PowerUsage,
                    Max = (double) state.PowerLimit
                },
                Temperature = new Heartbeat.ValueWithLimits<int>
                {
                    Current = (int) state.Temperature,
                    Max = (int) state.MaxTemperature
                },
                Utilization = (int) state.GpuUtilization
            };
    }
}
