namespace Msv.AutoMiner.Rig.Commands
{
    public interface ICommandProcessor
    {
        [CommandAction("--list-algorithms", Description = "List all algorithms known by rig")]
        void ListAlgorithms();

        [CommandAction("--list-miners", Description = "List all miners known by rig")]
        void ListMiners();

        [CommandAction("--assign-miner-exe", Description = "Set miner executable path")]
        void AssignMinerExecutable(
            [CommandParameter("-m", Description = "miner ID", Example = "3", IsRequired = true)] int minerId,
            [CommandParameter("-path", Description = "executable path", Example = "\"D:\\Miners\\miner.exe\"", IsRequired = true)] string path,
            [CommandParameter("-sec", Description = "secondary executable path", Example = "\"D:\\Miners\\miner.exe\"")] string secondary);

        [CommandAction("--show-miner-info", Description = "Show version information of the miner executable")]
        void ShowMinerVersionInfo(
            [CommandParameter("-m", Description = "miner ID", Example = "3", IsRequired = true)] int minerId);

        [CommandAction("--set-algorithm-options", Description = "Set miner options for specified algorithm")]
        void SetAlgorithmOptions(
            [CommandParameter("-algo", Description = "algorithm name", Example = "skunk")] string algorithm,
            [CommandParameter("-miner", Description = "miner ID", Example = "2")] int? minerId,
            [CommandParameter("-arg", Description = "miner algorithm argument", Example = "skunk")] string minerArgument,
            [CommandParameter("-i", Description = "intensity", Example = "20.5")] double? intensity,
            [CommandParameter("-log", Description = "log file", Example = "/var/log/mining.log")] string logFile);

        [CommandAction("--list-gpu-devices", Description = "List all available GPU devices")]
        void ListGpuDevices();

        [CommandAction("--list-manual-mappings", Description = "List all current manual mappings")]
        void ListManualMappings();

        [CommandAction("--set-manual-mapping",
            Description = "Set manual mapping to assign devices to mine particular coin")]
        void SetManualMapping(
            [CommandParameter("-d", Description = "device IDs", Example = "1,2,3", IsRequired = true)] int[] deviceIds,
            [CommandParameter("-c", Description = "coin symbol", Example = "BTC", IsRequired = true)] string coinSymbol);

        [CommandAction("--clear-manual-mapping", Description = "Clear manual mapping for specified device")]
        void ClearManualMapping(
            [CommandParameter("-d", Description = "device IDs", Example = "1,2,3")] int[] deviceIds);

        [CommandAction("--register", Description = "Register this rig at the control center service")]
        void Register(
            [CommandParameter("-name", Description = "rig name", Example = "test", IsRequired = true)] string name,
            [CommandParameter("-pass", Description = "registration password", Example = "123456", IsRequired = true)] string password);

        [CommandAction("--test", Description = "Run algorithm speed tests with active coins")]
        void Test(
            [CommandParameter("-a", Description = "algorithms to test", Example = "sha256t,groestl")]
            string[] algorithms,
            [CommandParameter("-c", Description = "coins to test", Example = "xcn,zcash")]
            string[] coinNames);

        [CommandAction("--config-env", Description = "Configure .NET runtime to run this program properly")]
        void ConfigureEnvironment();
    }
}
