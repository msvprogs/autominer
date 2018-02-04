using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Remote;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class ConfigurationUpdater
    {        
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IPeriodicTaskDelayProvider m_DelayProvider;
        private readonly IControlCenterService m_Service;
        private readonly IConfigurationHasher m_ConfigurationHasher;
        private readonly IMinerFileStorage m_MinerFileStorage;
        private readonly IConfigurationUpdaterStorage m_Storage;

        public ConfigurationUpdater(
            IPeriodicTaskDelayProvider delayProvider,
            IControlCenterService service,
            IConfigurationHasher configurationHasher,
            IMinerFileStorage minerFileStorage,
            IConfigurationUpdaterStorage storage)
        {
            m_DelayProvider = delayProvider ?? throw new ArgumentNullException(nameof(delayProvider));
            m_Service = service ?? throw new ArgumentNullException(nameof(service));
            m_ConfigurationHasher = configurationHasher ?? throw new ArgumentNullException(nameof(configurationHasher));
            m_MinerFileStorage = minerFileStorage ?? throw new ArgumentNullException(nameof(minerFileStorage));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public bool CheckUpdates()
        {
            M_Logger.Info("Checking configuration updates...");
            var delay = m_DelayProvider.GetDelay<ConfigurationUpdater>();
            M_Logger.Info($"Waiting {delay.TotalSeconds:F0} seconds...");
            Thread.Sleep(delay);
            M_Logger.Info("Hashing local configuration...");
            // ReSharper disable CoVariantArrayConversion
            var hash = m_ConfigurationHasher.Calculate(m_Storage.GetMiners(), m_Storage.GetMinerAlgorithmSettings());
            // ReSharper restore CoVariantArrayConversion
            M_Logger.Info("Local configuration hash: " + HexHelper.ToHex(hash));

            M_Logger.Info("Checking server configuration hash...");
            var response = m_Service.CheckConfiguration(new GetConfigurationRequestModel
            {
                Platform = GetCurrentPlatform()
            });
            M_Logger.Info("Server configuration hash: " + HexHelper.ToHex(response.ConfigurationHash));
            if (hash.SequenceEqual(response.ConfigurationHash))
            {
                M_Logger.Info("There are no configuration updates");
                return false;
            }
            M_Logger.Info("Configuration update found!");
            return true;
        }

        public void ApplyUpdates()
        {
            M_Logger.Info("Applying updates...");
            var delay = m_DelayProvider.GetDelay<ConfigurationUpdater>();
            M_Logger.Info($"Waiting {delay.TotalSeconds:F0} seconds...");
            Thread.Sleep(delay);
            M_Logger.Info("Downloading new configuration...");
            var response = m_Service.GetConfiguration(
                new GetConfigurationRequestModel
                {
                    Platform = GetCurrentPlatform()
                });
            M_Logger.Info($"Got {response.Miners.Length} miners data, {response.Algorithms.Length} algorithms, downloading and applying them...");
            if (response.Miners.Any())
                m_Storage.SaveMiners(DownloadAndConvertMiners(response.Miners));
            if (response.Algorithms.Any())
            {
                M_Logger.Info("Storing new algorithm info...");
                m_Storage.SaveAlgorithms(response.Algorithms
                    .Select(x => new AlgorithmData
                    {
                        AlgorithmId = x.AlgorithmId.ToString(),
                        AlgorithmName = x.AlgorithmName
                    })
                    .ToArray());
                m_Storage.SaveMinerAlgorithmSettings(
                    response.Algorithms
                        .Select(x => new MinerAlgorithmSetting
                        {
                            AlgorithmId = x.AlgorithmId.ToString(),
                            AdditionalArguments = x.AdditionalArguments,
                            AlgorithmArgument = x.AlgorithmArgument,
                            Intensity = x.Intensity,
                            MinerId = x.MinerId
                        })
                        .ToArray());
            }
            M_Logger.Info("Configuration update is completed");
        }

        private Miner[] DownloadAndConvertMiners(IEnumerable<MinerModel> newMiners) 
            => newMiners
                .Select(x => new
                {
                    MinerModel = x,
                    Path = m_MinerFileStorage.GetPath(x.VersionId) 
                           ?? (x.MainExecutableName != null
                                ? m_MinerFileStorage.Save(m_Service.DownloadMiner(x.VersionId), x.MinerName, x.VersionId, x.MainExecutableName)
                                : null)
                })
                .Select(x => new Miner
                {
                    Name = x.MinerModel.MinerName,
                    Id = x.MinerModel.MinerId,
                    Version = x.MinerModel.Version,
                    AlgorithmArgument = x.MinerModel.AlgorithmArgument,
                    ServerArgument = x.MinerModel.ServerArgument,
                    InvalidShareRegex = x.MinerModel.InvalidShareRegex,
                    VersionId = x.MinerModel.VersionId,
                    SpeedRegex = x.MinerModel.SpeedRegex,
                    PasswordArgument = x.MinerModel.PasswordArgument,
                    AdditionalArguments = x.MinerModel.AdditionalArguments,
                    BenchmarkArgument = x.MinerModel.BenchmarkArgument,
                    BenchmarkResultRegex = x.MinerModel.BenchmarkResultRegex,
                    OmitUrlSchema = x.MinerModel.OmitUrlSchema,
                    ValidShareRegex = x.MinerModel.ValidShareRegex,
                    IntensityArgument = x.MinerModel.IntensityArgument,
                    UserArgument = x.MinerModel.UserArgument,
                    PortArgument = x.MinerModel.PortArgument,
                    IsDownloaded = x.Path != null,
                    ApiPortArgument = x.MinerModel.ApiPortArgument,
                    ApiType = x.MinerModel.ApiType,
                    ApiPort = x.MinerModel.ApiPort,
                    FileName = x.MinerModel.ExeSecondaryFilePath != null
                        ? x.MinerModel.ExeFilePath
                        : x.Path != null 
                            ? Path.Combine(x.Path, x.MinerModel.ExeFilePath)
                            : x.MinerModel.ExeFilePath,
                    SecondaryFileName = x.MinerModel.ExeSecondaryFilePath != null
                        ? x.Path != null 
                            ? Path.Combine(x.Path, x.MinerModel.ExeSecondaryFilePath)
                            : x.MinerModel.ExeSecondaryFilePath
                        : null
                })
                .ToArray();

        private static PlatformType GetCurrentPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return PlatformType.Windows;
                case PlatformID.Unix:
                    return PlatformType.Linux;
                default:
                    throw new PlatformNotSupportedException();
            }
        }
    }
}
