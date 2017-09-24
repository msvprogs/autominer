using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NLog;

namespace Msv.AutoMiner.Common
{
    public abstract class MonitorBase : IDisposable
    {
        public TimeSpan Period { get; }

        protected ILogger Log { get; }

        private readonly IDisposable m_Disposable;

        protected MonitorBase(TimeSpan period, bool skipFirst = false)
        {
            Period = period;
            Log = LogManager.GetLogger(GetType().Name);

            var sequence = Observable.Timer(period).Repeat();
            if (!skipFirst)
                sequence = sequence.StartWith(TaskPoolScheduler.Default, 0);
            m_Disposable = sequence
                .Subscribe(x => DoWorkWrapped(), x => Log.Fatal(x, "Something strange happened"));
        }

        public void Dispose() => m_Disposable.Dispose();

        protected abstract void DoWork();

        private void DoWorkWrapped()
        {
            try
            {
                Log.Info("Starting work...");
                DoWork();
                Log.Info("Work completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception occurred");
            }
        }
    }
}
