using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using NLog;

namespace Msv.AutoMiner.Common
{
    public abstract class MonitorBase : IDisposable
    {
        private static readonly TimeSpan M_WorkTimeout = TimeSpan.FromMinutes(10);

        public TimeSpan Period { get; }

        protected ILogger Log { get; }

        private readonly IDisposable m_Disposable;

        protected MonitorBase(TimeSpan period, TimeSpan? delay = null, bool skipFirst = false)
        {
            Period = period;
            Log = LogManager.GetLogger(GetType().Name);

            var sequence = Observable.Interval(period, TaskPoolScheduler.Default);
            if (!skipFirst)
                sequence = sequence.StartWith(TaskPoolScheduler.Default, 0);

            if (delay != null)
            {
#if !DEBUG
                Log.Info($"Adding server load balancing delay {(int)delay.Value.TotalSeconds} seconds");
                sequence = sequence.Delay(delay.Value);
#endif
            }

            m_Disposable = sequence
                .Subscribe(
                    x => DoWorkWrapped(),
                    x => Log.Fatal(x, "Something strange happened"),
                    () => Log.Fatal("Sequence ended - it wasn't supposed to happen"));
        }

        public void Dispose() => m_Disposable.Dispose();

        protected abstract void DoWork();

        private void DoWorkWrapped()
        {
            try
            {
                Log.Info("Starting work...");
                var thread = new Thread(x =>
                {
                    try
                    {
                        DoWork();
                    }
                    catch (ThreadAbortException)
                    {
                        Log.Error("Thread has been aborted - probably timeout exceeded?");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Exception occurred");
                    }
                }) { IsBackground = true };
                thread.Start();
                if (thread.Join(M_WorkTimeout))
                    Log.Info("Work completed");
                else
                {
                    Log.Warn("Work aborted - timeout exceeded");
                    thread.Abort();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception occurred");
            }
        }
    }
}
