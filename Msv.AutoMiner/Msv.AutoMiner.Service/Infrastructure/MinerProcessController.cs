using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.System.Contracts;
using NLog;
using System.Linq;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure.Data;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class MinerProcessController : IMinerProcessController
    {
        public MiningMode CurrentMode { get; private set; }
        public CoinMiningInfo[] CurrentCoins
            => m_CurrentCoins?.Select((x, i) => new CoinMiningInfo(
                    x, CurrentMode == MiningMode.Double && i == 1 
                ? m_MinerOutputProcessor?.CurrentSecondaryHashRate 
                : m_MinerOutputProcessor?.CurrentHashRate,
                    m_AlgorithmDatas.TryGetValue(x.Algorithm)?.SpeedInHashes,
                    m_MinerOutputProcessor?.AcceptedShares))
                .ToArray();

        private const int MaxRestartAttempts = 20;
        private static readonly ILogger M_Logger = LogManager.GetLogger("MinerProcessController");

        private readonly IEnvironmentVariableCreator m_VariableCreator;
        private readonly IProcessStopper m_ProcessStopper;
        private readonly IChildProcessTracker m_ProcessTracker;
        private readonly TimeSpan m_ShareTimeout;
        private readonly Dictionary<CoinAlgorithm, AlgorithmData> m_AlgorithmDatas;
        private readonly object m_SyncRoot = new object();
        private readonly SerialDisposable m_CurrentProcessDisposable = new SerialDisposable();

        private long m_LastLogPosition;
        private IMinerOutputProcessor m_MinerOutputProcessor;
        private Coin[] m_CurrentCoins;

        public event EventHandler ProcessExited;

        public MinerProcessController(
            IEnvironmentVariableCreator variableCreator,
            IProcessStopper processStopper,
            IChildProcessTracker processTracker,
            TimeSpan shareTimeout,
            AlgorithmData[] algorithmDatas)
        {
            if (variableCreator == null)
                throw new ArgumentNullException(nameof(variableCreator));
            if (processStopper == null)
                throw new ArgumentNullException(nameof(processStopper));
            if (processTracker == null)
                throw new ArgumentNullException(nameof(processTracker));
            if (algorithmDatas == null)
                throw new ArgumentNullException(nameof(algorithmDatas));

            m_VariableCreator = variableCreator;
            m_ProcessStopper = processStopper;
            m_ProcessTracker = processTracker;
            m_ShareTimeout = shareTimeout;
            m_AlgorithmDatas = algorithmDatas.ToDictionary(x => x.Algorithm);
        }

        public void RunNew(Coin[] coins,  Miner miner)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));
            if (miner == null)
                throw new ArgumentNullException(nameof(miner));
            if (coins.Any(x => x.Pool == null))
                throw new InvalidOperationException("Can't mine the coin without associated mining pool");
            try
            {
                MiningMode mode;
                if (coins.Length == 1)
                    mode = MiningMode.Single;
                else if (coins.Select(x => x.Algorithm).Distinct().Count() == 1)
                    mode = MiningMode.Merged;
                else
                    mode = MiningMode.Double;
                RunNew(coins, mode, miner, MaxRestartAttempts);
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
            m_CurrentCoins = null;
        }

        public void Dispose() => m_CurrentProcessDisposable.Dispose();

        private void RunNew(Coin[] coins, MiningMode mode, Miner miner, int attempts)
        {
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
                var coin = coins.First();
                var outputLogFile = miner.ReadOutputFromLog && !string.IsNullOrEmpty(coin.Pool.LogFile)
                    ? coin.Pool.LogFile
                    : null;
                m_MinerOutputProcessor = new MinerOutputProcessor(
                    Path.GetFileNameWithoutExtension(file.Name),
                    miner.SpeedRegex, miner.ValidShareRegex, coin.CurrencySymbol,
                    mode == MiningMode.Double
                        ? coins.Skip(1).FirstOrDefault()?.CurrencySymbol
                        : null);
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

                var arguments = GetMinerArgumentString(coins, mode, miner);
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
                            RunNew(coins, mode, miner, --attempts);
                        },
                        x => m_CurrentProcessDisposable.Disposable = null));
                if (!string.IsNullOrEmpty(miner.ValidShareRegex) 
                    && coin.Pool.Protocol == PoolProtocol.Stratum)
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
                                RunNew(coins, mode, miner, --attempts);
                            },
                            x => m_CurrentProcessDisposable.Disposable = null));
                var pid = process.Start();
                M_Logger.Info($"Process \"{file.Name}\" started, PID={pid}");
                m_CurrentCoins = coins;
                CurrentMode = mode;
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

        private static string GetMinerArgumentString(Coin[] coins, MiningMode mode, Miner miner)
        {
            var coin = coins.First();
            var alternateCoin = mode == MiningMode.Double ? coins.Skip(1).FirstOrDefault() : null;
            var argumentValues = new List<string>();

            if (!string.IsNullOrWhiteSpace(miner.SecondaryFileName))
                argumentValues.Add($"\"{miner.SecondaryFileName}\"");
            AddServerArgumentValues(
                argumentValues, coin, miner.ServerArgument, miner.PortArgument, miner.UserArgument, miner.PasswordArgument);
            if (miner.IntensityArgument != null && coin.Pool.Intensity != null)
                argumentValues.Add(GetArgumentValuePair(
                    miner.IntensityArgument, 
                    coin.Pool.Intensity.Value.ToString("F2", CultureInfo.InvariantCulture)));
            if (miner.DifficultyMultiplierArgument != null && coin.Pool.DifficultyMultiplier != null)
                argumentValues.Add(GetArgumentValuePair(
                    miner.DifficultyMultiplierArgument,
                    coin.Pool.DifficultyMultiplier.Value.ToString()));
            if (alternateCoin != null && miner.AlternativeServerArgument != null &&
                miner.AlternativeUserArgument != null)
                AddServerArgumentValues(
                    argumentValues, alternateCoin, miner.AlternativeServerArgument,
                    null, miner.AlternativeUserArgument, miner.AlternativePasswordArgument);
            if (miner.AlgorithmArgument != null)
            {
                var algorithmValue = miner.AlgorithmValues
                    .FirstOrDefault(x => x.Algorithm == coin.Algorithm || x.Algorithm == alternateCoin?.Algorithm);
                if (algorithmValue != null)
                    argumentValues.Add(GetArgumentValuePair(miner.AlgorithmArgument, algorithmValue.Value));
            }

            var logFile = coin.Pool.LogFile?.Trim('"');
            if (miner.LogFileArgument != null && !string.IsNullOrEmpty(logFile))
                argumentValues.Add(GetArgumentValuePair(miner.LogFileArgument, $"\"{logFile}\""));
            return string.Join(" ", argumentValues.Concat(new[] {miner.OtherArguments}));
        }

        private static void AddServerArgumentValues(
            ICollection<string> args, Coin coin, string serverKey, string portKey, string userKey, string passwordKey)
        {
            var host = Uri.IsWellFormedUriString(coin.Pool.Address, UriKind.Absolute)
                ? new Uri(coin.Pool.Address).Host
                : coin.Pool.Address;
            if (!coin.Pool.Miner.OmitUrlSchema)
            {
                var addressBuilder = new UriBuilder { Host = host };
                if (portKey == null)
                    addressBuilder.Port = coin.Pool.Port;
                switch (coin.Pool.Protocol)
                {
                    case PoolProtocol.Stratum:
                        addressBuilder.Scheme = "stratum+tcp";
                        break;
                    case PoolProtocol.JsonRpc:
                        addressBuilder.Scheme = Uri.UriSchemeHttp;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(coin), "Unknown pool protocol");
                }
                args.Add(GetArgumentValuePair(serverKey, addressBuilder.Uri.ToString()));
            }
            else
                args.Add(GetArgumentValuePair(serverKey, portKey == null ? $"{host}:{coin.Pool.Port}" : host));

            if (portKey != null)
                args.Add(GetArgumentValuePair(portKey, coin.Pool.Port.ToString()));

            args.Add(GetArgumentValuePair(userKey, coin.Pool.GetLogin(coin)));
            if (passwordKey != null)
                args.Add(GetArgumentValuePair(passwordKey, coin.Pool.GetPassword()));
        }

        private static string GetArgumentValuePair(string argument, string value)
            => argument.EndsWith("=") ? argument + value : argument + " " + value;
    }
}

