using System;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class LogFileReader : IDisposable
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly CompositeDisposable m_Disposable = new CompositeDisposable();
        private readonly IMinerOutputProcessor m_MinerOutputProcessor;

        private long m_LastLogPosition;

        public LogFileReader(string logFilePath, IMinerOutputProcessor minerOutputProcessor)
        {
            if (logFilePath == null) 
                throw new ArgumentNullException(nameof(logFilePath));
            m_MinerOutputProcessor = minerOutputProcessor ?? throw new ArgumentNullException(nameof(minerOutputProcessor));

            var outputFile = new FileInfo(logFilePath);
            if (outputFile.DirectoryName == null || !Path.IsPathRooted(logFilePath))
                throw new ArgumentException("Invalid path to log file. It must be absolute.");

            m_LastLogPosition = outputFile.Exists ? outputFile.Length : 0;
            var fileSystemWatcher = new FileSystemWatcher(outputFile.DirectoryName)
            {
                EnableRaisingEvents = true
            };
            m_Disposable.Add(fileSystemWatcher);
            m_Disposable.Add(CreateSubscription(logFilePath, fileSystemWatcher));
        }

        public void Dispose() 
            => m_Disposable.Dispose();

        private IDisposable CreateSubscription(string outputLogFile, FileSystemWatcher watcher)
        {
            var shortFileName = Path.GetFileName(outputLogFile);
            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    x => watcher.Changed += x, x => watcher.Changed -= x)
                .Merge(Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    x => watcher.Created += x, x => watcher.Created -= x))
                .Where(x => x.EventArgs.Name == shortFileName)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Subscribe(x => ProcessFileChangedEvent(outputLogFile));
        }

        private void ProcessFileChangedEvent(string outputLogFile)
        {
            try
            {
                using (var logFile = new FileStream(outputLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new StreamReader(logFile))
                {
                    if (m_LastLogPosition > logFile.Length)
                        m_LastLogPosition = 0;
                    logFile.Seek(m_LastLogPosition, SeekOrigin.Begin);
                    var log = reader.ReadToEnd().Trim();
                    m_LastLogPosition = logFile.Length;
                    if (!string.IsNullOrEmpty(log))
                        m_MinerOutputProcessor.Write(log);
                }
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex, "Log file I/O error");
            }
        }
    }
}
