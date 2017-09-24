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

        protected MonitorBase(TimeSpan period)
        {
            Period = period;
            Log = LogManager.GetLogger(GetType().Name);

            m_Disposable = Observable.Timer(period)
                .Repeat()
                .StartWith(TaskPoolScheduler.Default, 0)
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
