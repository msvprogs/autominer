using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Remote;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;
using Msv.AutoMiner.Rig.System.Video;

namespace Msv.AutoMiner.Rig.Commands
{
    public class CommandProcessor : ICommandProcessor
    {
        private readonly IVideoSystemStateProvider m_VideoStateProvider;
        private readonly IControlCenterService m_Service;
        private readonly IControlCenterRegistrator m_Registrator;
        private readonly IMinerTester m_Tester;
        private readonly ICommandProcessorStorage m_Storage;

        public CommandProcessor(
            IVideoSystemStateProvider videoStateProvider,
            IControlCenterService service,
            IControlCenterRegistrator registrator,
            IMinerTester tester,
            ICommandProcessorStorage storage)
        {
            m_VideoStateProvider = videoStateProvider ?? throw new ArgumentNullException(nameof(videoStateProvider));
            m_Service = service ?? throw new ArgumentNullException(nameof(service));
            m_Registrator = registrator ?? throw new ArgumentNullException(nameof(registrator));
            m_Tester = tester ?? throw new ArgumentNullException(nameof(tester));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public void ListAlgorithms()
        {
            var builder = new TableStringBuilder("Algorithm name", "Current miner", "Current hashrate", "Current power");
            var algorithmSettings = m_Storage.GetMinerAlgorithmSettings();
            m_Storage.GetAlgorithms()
                .OrderBy(x => x.AlgorithmName)
                .LeftOuterJoin(algorithmSettings, x => x.AlgorithmId, x => x.AlgorithmId, (x, y) => (algorithm:x, miner:y))
                .ForEach(x => builder.AppendValues(
                    x.algorithm.AlgorithmName, 
                    x.miner?.Miner?.Name ?? "--", 
                    ConversionHelper.ToHashRateWithUnits(x.algorithm.SpeedInHashes, x.algorithm.KnownValue), 
                    $"{x.algorithm.Power:F2} W"));
            Console.WriteLine(builder);
        }

        public void UpdateAlgorithms()
        {
            Console.WriteLine("Requesting algorithms from server...");
            var serverAlgorithms = m_Service.GetAlgorithms();
            var localAlgorithms = m_Storage.GetAlgorithms();
            var newAlgorithms = serverAlgorithms.LeftOuterJoin(localAlgorithms,
                    x => x.Id, x => Guid.Parse(x.AlgorithmId), (x, y) => y == null ? x : null)
                .Where(x => x != null)
                .ToArray();
            Console.WriteLine(
                $"Got {serverAlgorithms.Length} algorithms, already have {localAlgorithms.Length}, {newAlgorithms.Length} new");
            if (!newAlgorithms.Any())
                return;
            m_Storage.StoreAlgorithms(newAlgorithms.Select(x =>
                    new AlgorithmData
                    {
                        AlgorithmId = x.Id.ToString(),
                        AlgorithmName = x.Name,
                        KnownValue = x.KnownValue
                    })
                .ToArray());
        }

        public void ListMiners()
        {
            var builder = new TableStringBuilder("ID", "Name", "Path", "File exists?");
            m_Storage.GetMiners().ForEach(x => builder.AppendValues(
                x.Id,
                x.Name,
                string.Join(" ", new[] {x.FileName, x.SecondaryFileName}.Where(y => y != null)),
                File.Exists(x.FileName) && (x.SecondaryFileName == null || File.Exists(x.SecondaryFileName))
                    ? "Yes"
                    : "No"));
            Console.WriteLine(builder);
        }

        public void AssignMinerExecutable(int minerId, string path, string secondary)
        {
            var miner = m_Storage.GetMiners().FirstOrDefault(x => x.Id == minerId);
            if (miner == null)
            {
                Console.WriteLine($"Miner with ID {minerId} not found");
                return;
            }
            if (!File.Exists(path))
            {
                Console.WriteLine($"Executable {path} doesn't exist!");
                return;
            }
            miner.FileName = path;
            if (secondary != null && !File.Exists(secondary))
            {
                Console.WriteLine($"Secondary executable {secondary} doesn't exist!");
                return;
            }
            miner.SecondaryFileName = secondary;
            m_Storage.SaveMiner(miner);
            Console.WriteLine($"Miner {miner.Name} was assigned the executable path: {path} {secondary}");
        }

        public void ShowMinerVersionInfo(int minerId)
        {
            var miner = m_Storage.GetMiners().FirstOrDefault(x => x.Id == minerId);
            if (miner == null)
            {
                Console.WriteLine($"Miner with ID {minerId} not found");
                return;
            }
            Console.WriteLine($"Miner executable: {miner.FileName}");
            ShowExeInfo(miner.FileName);
            if (miner.SecondaryFileName != null)
            {
                Console.WriteLine($"Miner secondary executable: {miner.SecondaryFileName}");
                ShowExeInfo(miner.SecondaryFileName);
            }

            void ShowExeInfo(string path)
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"ATTENTION: File {path} not found!");
                    return;
                }
                var info = FileVersionInfo.GetVersionInfo(path);
                Console.WriteLine($@"File version: {info.FileVersion}
Product name: {info.ProductName}
Product version: {info.ProductVersion}
Company: {info.CompanyName}
Copyright: {info.LegalCopyright}");
            }
        }

        public void SetAlgorithmOptions(string algorithm, int? minerId, string minerArgument, double? intensity, string logFile)
        {
            if (algorithm == null)
                throw new ArgumentNullException(nameof(algorithm));

            var options = m_Storage.GetMinerAlgorithmSetting(algorithm);
            if (options == null)
            {
                Console.WriteLine($"Unknown algorithm: {algorithm}. Try to update algorithm list from server.");
                return;
            }
            if (minerId != null)
                options.MinerId = minerId.Value;
            if (minerArgument != null)
                options.AlgorithmArgument = minerArgument;
            if (intensity != null)
                options.Intensity = intensity <= 0 ? null : intensity;
            if (logFile != null)
                options.LogFile = logFile;
            m_Storage.SaveMinerAlgorithmSetting(options);
            Console.WriteLine($"Current options for algorithm {algorithm}: miner {options.MinerId}, " 
                + $"algorithm argument '{options.AlgorithmArgument}', intensity {options.Intensity}, log file '{options.LogFile}'");
        }

        public void ListGpuDevices()
        {
            if (!m_VideoStateProvider.CanUse)
            {
                Console.WriteLine("Graphic adapter info is unavailable");
                return;
            }
            var gpus = m_VideoStateProvider.GetState()?.AdapterStates;
            if (gpus == null || !gpus.Any())
            {
                Console.WriteLine("No graphic adapters found");
                return;
            }

            var builder = new TableStringBuilder("ID", "Name");
            gpus.ForEach(x => builder.AppendValues(x.Index, x.Name));
            Console.WriteLine(builder);
        }

        public void ListManualMappings()
        {
            var builder = new TableStringBuilder("Device type", "Device ID", "Mapped coin");
            m_Storage.GetManualMappings()
                .ForEach(x => builder.AppendValues(x.DeviceType, x.DeviceType, x.CurrencySymbol));
            Console.WriteLine(builder);
        }

        public void SetManualMapping(int[] deviceIds, string coinSymbol)
        {
            if (deviceIds == null)
                throw new ArgumentNullException(nameof(deviceIds));
            if (coinSymbol == null)
                throw new ArgumentNullException(nameof(coinSymbol));

            m_Storage.SaveManualMappings(deviceIds.Select(x => new ManualDeviceMapping
            {
                DeviceId = x,
                DeviceType = DeviceType.Gpu,
                CurrencySymbol = coinSymbol
            })
            .ToArray());
            Console.WriteLine($"Devices {string.Join(", ", deviceIds)} mapped to coin {coinSymbol}");
        }

        public void ClearManualMapping(int[] deviceIds)
        {
            m_Storage.ClearManualMappings(deviceIds
                .EmptyIfNull()
                .Select(x => new KeyValuePair<DeviceType, int>(DeviceType.Gpu, x))
                .ToArray());
            Console.WriteLine($"Mappings for devices {string.Join(", ", deviceIds)} cleared");
        }

        public void Register(string name, string password)
            => m_Registrator.Register(name, password);

        public void Test(string[] algorithms, string[] coinNames)
            => m_Tester.Test(algorithms, coinNames);
    }
}
