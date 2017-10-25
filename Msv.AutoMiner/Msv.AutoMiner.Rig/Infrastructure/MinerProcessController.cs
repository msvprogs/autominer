using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Rig.Data;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;
using Msv.AutoMiner.Rig.System.Contracts;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class MinerProcessController : IMinerProcessController
    {
        public MiningState CurrentState
            => m_CurrentMiningData == null
                ? null
                : new MiningState(m_CurrentMiningData, 
                    m_MinerOutputProcessor?.CurrentHashRate,
                    m_AlgorithmDatas.FirstOrDefault(x => x.AlgorithmId == m_CurrentMiningData.MinerSettings.AlgorithmId)?.SpeedInHashes,
                    m_MinerOutputProcessor?.AcceptedShares,
                    m_MinerOutputProcessor?.RejectedShares);

        public DateTime StateChanged { get; private set; }

        private const int MaxRestartAttempts = 20;
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnvironmentVariableCreator m_VariableCreator;
        private readonly IProcessStopper m_ProcessStopper;
        private readonly IChildProcessTracker m_ProcessTracker;
        private readonly TimeSpan m_ShareTimeout;
        private readonly AlgorithmData[] m_AlgorithmDatas;
        private readonly object m_SyncRoot = new object();
        private readonly SerialDisposable m_CurrentProcessDisposable = new SerialDisposable();

        private long m_LastLogPosition;
        private IMinerOutputProcessor m_MinerOutputProcessor;
        private CoinMiningData m_CurrentMiningData;

        public event EventHandler ProcessExited;

        public MinerProcessController(
            IEnvironmentVariableCreator variableCreator,
            IProcessStopper processStopper,
            IChildProcessTracker processTracker,
            TimeSpan shareTimeout,
            AlgorithmData[] algorithmDatas)
        {
            m_VariableCreator = variableCreator ?? throw new ArgumentNullException(nameof(variableCreator));
            m_ProcessStopper = processStopper ?? throw new ArgumentNullException(nameof(processStopper));
            m_ProcessTracker = processTracker ?? throw new ArgumentNullException(nameof(processTracker));
            m_ShareTimeout = shareTimeout;
            m_AlgorithmDatas = algorithmDatas ?? throw new ArgumentNullException(nameof(algorithmDatas));
            StateChanged = DateTime.UtcNow;
        }

        public void RunNew(CoinMiningData miningData)
        {
            if (miningData == null)
                throw new ArgumentNullException(nameof(miningData));
            try
            {
                RunNew(miningData, MaxRestartAttempts);
            }
            catch
            {
                Stop();
                throw;
            }
        }

        public void Stop()
        {
            m_CurrentProcessDisposable.Disposable = null;
            m_CurrentMiningData = null;
        }

        public void Dispose() => m_CurrentProcessDisposable.Dispose();

        private void RunNew(CoinMiningData miningData, int attempts)
        {
            var miner = miningData.MinerSettings.Miner;
            var file = new FileInfo(miner.FileName);
            if (attempts == 0)
            {
                M_Logger.Error($"Couldn't run process \"{file.Name}\": attempts exceeded");
                ProcessExited?.Invoke(this, EventArgs.Empty);
                return;
            }
            lock (m_SyncRoot)
            {
                M_Logger.Info("Stopping current miner process, if any...");
                var newDisposable = new CompositeDisposable();
                m_CurrentProcessDisposable.Disposable = newDisposable;
                var outputLogFile = miner.ReadOutputFromLog && !string.IsNullOrEmpty(miningData.MinerSettings.LogFile)
                    ? miningData.MinerSettings.LogFile
                    : null;
                m_MinerOutputProcessor = new MinerOutputProcessor(
                    Path.GetFileNameWithoutExtension(file.Name), miner, miningData.CoinSymbol, null, miningData.BenchmarkMode);
                if (outputLogFile != null)
                {
                    var directory = Path.GetDirectoryName(outputLogFile);
                    if (directory == null)
                        throw new ArgumentException("Invalid path to log file. It must be absolute.");
                    M_Logger.Debug($"Starting to listen to log file \"{outputLogFile}\"");
                    var outputFile = new FileInfo(outputLogFile);
                    m_LastLogPosition = outputFile.Exists ? outputFile.Length : 0;
                    var fileSystemWatcher = new FileSystemWatcher(directory)
                    {
                        EnableRaisingEvents = true
                    };
                    var watcherSubscription = CreateLogFileWatcherSubscription(outputLogFile, fileSystemWatcher);
                    newDisposable.Add(watcherSubscription);
                    newDisposable.Add(fileSystemWatcher);
                }

                var arguments = GetMinerArgumentString(miningData);
                M_Logger.Debug($"Running new miner process: \"{file.FullName}\" {arguments}");
                var process = new ProcessWrapper(
                    file, arguments, m_VariableCreator, m_ProcessStopper,
                    outputLogFile == null ? m_MinerOutputProcessor : null, m_ProcessTracker);
                newDisposable.Add(process);
                newDisposable.Add(Observable.FromEventPattern(x => process.Exited += x, x => process.Exited -= x)
                    .Take(1)
                    .Subscribe(x =>
                        {
                            M_Logger.Warn(
                                $"Process \"{file.Name}\" has exited on its own, restarting it (remaining {attempts} attempts)...");
                            RunNew(miningData, --attempts);
                        },
                        x => m_CurrentProcessDisposable.Disposable = null));
                if (!miningData.BenchmarkMode 
                    && !string.IsNullOrEmpty(miner.ValidShareRegex)
                    && miningData.PoolData?.Protocol == PoolProtocol.Stratum) //TODO: disable share checking for solomining & benchmark mode
                    newDisposable.Add(Observable.Interval(TimeSpan.FromSeconds(10))
                        .Select(x => m_MinerOutputProcessor.AcceptedShares)
                        .DistinctUntilChanged()
                        .Throttle(m_ShareTimeout)
                        .Take(1)
                        .Subscribe(x =>
                            {
                                M_Logger.Warn(
                                    $"Process \"{file.Name}\" hasn't generated any shares in last 3 minutes, restarting it (remaining {attempts} attempts)...");
                                process.Stop(true);
                                RunNew(miningData, --attempts);
                            },
                            x => m_CurrentProcessDisposable.Disposable = null));
                var pid = process.Start();
                M_Logger.Info($"Process \"{file.Name}\" started, PID={pid}");
                StateChanged = DateTime.UtcNow;
                m_CurrentMiningData = miningData;
            }
        }

        private IDisposable CreateLogFileWatcherSubscription(
            string outputLogFile, 
            FileSystemWatcher fileSystemWatcher)
        {
            var shortFileName = Path.GetFileName(outputLogFile);
            return Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    x => fileSystemWatcher.Changed += x, x => fileSystemWatcher.Changed -= x)
                .Merge(Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    x => fileSystemWatcher.Created += x, x => fileSystemWatcher.Created -= x))
                .Where(x => x.EventArgs.Name == shortFileName)
                .Throttle(TimeSpan.FromMilliseconds(50))
                .Subscribe(x =>
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
                });
        }

        private static string GetMinerArgumentString(CoinMiningData miningData)
        {
            var miner = miningData.MinerSettings.Miner;
            var argumentValues = new List<string>();

            if (!string.IsNullOrWhiteSpace(miner.SecondaryFileName))
                argumentValues.Add($"\"{miner.SecondaryFileName}\"");

            if (miner.AlgorithmArgument != null && miningData.MinerSettings.AlgorithmArgument != null)
                argumentValues.Add(GetArgumentValuePair(miner.AlgorithmArgument, miningData.MinerSettings.AlgorithmArgument));

            AddServerArgumentValues(
                argumentValues, miningData, miner.ServerArgument, miner.PortArgument, miner.UserArgument, miner.PasswordArgument, miner.BenchmarkArgument);
            if (miner.IntensityArgument != null && miningData.MinerSettings.Intensity != null)
                argumentValues.Add(GetArgumentValuePair(
                    miner.IntensityArgument,
                    miningData.MinerSettings.Intensity.Value.ToString("F2", CultureInfo.InvariantCulture)));

            var logFile = miningData.MinerSettings.LogFile?.Trim('"');
            if (miner.LogFileArgument != null && !string.IsNullOrEmpty(logFile))
                argumentValues.Add(GetArgumentValuePair(miner.LogFileArgument, $"\"{logFile}\""));
            return string.Join(" ", argumentValues.Concat(
                new[] {miningData.MinerSettings.AdditionalArguments, miner.AdditionalArguments}));
        }

        private static void AddServerArgumentValues(
            ICollection<string> args, CoinMiningData miningData, string serverKey, string portKey, string userKey, string passwordKey, string benchmarkKey)
        {
            if (miningData.BenchmarkMode && miningData.PoolData == null)
            {
                args.Add(benchmarkKey);
                return;
            }
            args.Add(miningData.MinerSettings.Miner.OmitUrlSchema
                ? GetArgumentValuePair(serverKey,
                    portKey == null
                        ? $"{miningData.PoolData.Url.Host}:{miningData.PoolData.Url.Port}"
                        : miningData.PoolData.Url.Host)
                : GetArgumentValuePair(serverKey, miningData.PoolData.Url.ToString()));

            if (portKey != null)
                args.Add(GetArgumentValuePair(portKey, miningData.PoolData.Url.Port.ToString()));

            args.Add(GetArgumentValuePair(userKey, miningData.PoolData.Login));
            if (passwordKey != null)
                args.Add(GetArgumentValuePair(passwordKey, miningData.PoolData.Password));
        }

        private static string GetArgumentValuePair(string argument, string value)
            => argument.EndsWith("=") ? argument + value : argument + " " + value;
    }
}