using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.System.Video;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class VideoAdapterMonitor : IVideoAdapterMonitor
    {
        public bool IsAlive { get; private set; } = true;

        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();
        private readonly IVideoSystemStateProvider m_StateProvider;
        private readonly IDisposable m_Disposable;

        public VideoAdapterMonitor(IVideoSystemStateProvider stateProvider)
        {
            m_StateProvider = stateProvider ?? throw new ArgumentNullException(nameof(stateProvider));

            if (!stateProvider.CanUse)
            {
                M_Logger.Warn("Can't use video adapter monitoring on this system");
                return;
            }
            m_Disposable = Observable.Interval(TimeSpan.FromMinutes(2))
                .StartWith(Scheduler.Default, 0)
                .TakeWhile(x => IsAlive)
                .Select(x => GetCurrentState())
                .Do(x =>
                {
                    if (x == null)
                        return;
                    M_Logger.Info($"Current video adapters state (driver {x.DriverVersion}):{Environment.NewLine}"
                                  + string.Join(Environment.NewLine, x.AdapterStates.Select(y =>
                                      $"{y.Name}: Core {y.GpuClocksMhz}/{y.GpuMaxClocksMhz} MHz, memory {y.MemoryClocksMhz}/{y.MemoryMaxClocksMhz} MHz, "
                                      + $"GPU usage {y.GpuUtilization}%, temp {y.Temperature}°C/{y.MaxTemperature}°C, fan speed {y.FanSpeed}%, "
                                      + $"memory usage {y.UsedMemoryMb:N0} Mb/{y.TotalMemoryMb:N0} Mb, power usage {y.PowerUsage:F1}W/{y.PowerLimit:F1}W, state P{y.PerformanceState}")));
                })
                .Where(x => x == null)
                .Skip(2)
                .Subscribe(x =>
                {
                    M_Logger.Error("Video card monitoring API didn't return correct result within 3 attempts. "
                                   + "It means that video adapter has probably stopped functioning.");
                    IsAlive = false;
                });
        }

        public VideoSystemState GetCurrentState()
        {
            try
            {
                return m_StateProvider.CanUse ? m_StateProvider.GetState() : null;
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex, "Couldn't retreive video adapter state");
                return null;
            }
        }

        public void Dispose() => m_Disposable?.Dispose();
    }
}