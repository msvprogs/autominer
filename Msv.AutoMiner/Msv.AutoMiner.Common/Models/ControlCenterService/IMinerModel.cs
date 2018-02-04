using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public interface IMinerModel
    {
        int MinerId { get; }

        string MinerName { get; }

        int VersionId { get; }

        string Version { get; }

        string ExeFilePath { get; }

        string ExeSecondaryFilePath { get; }

        string ServerArgument { get; }

        string PortArgument { get; }

        string UserArgument { get; }

        string PasswordArgument { get; }

        string IntensityArgument { get; }

        string AlgorithmArgument { get; }

        string BenchmarkArgument { get; }

        string AdditionalArguments { get; }

        string SpeedRegex { get; }

        string ValidShareRegex { get; }

        string InvalidShareRegex { get; }

        string BenchmarkResultRegex { get; }

        string ApiPortArgument { get; }

        MinerApiType ApiType { get; }

        int? ApiPort { get; }

        bool OmitUrlSchema { get; }
    }
}
