using System.IO;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class MinerModel : IMinerModel
    {
        public int MinerId { get; set; }

        public string MinerName { get; set; }

        public int VersionId { get; set; }

        public string Version { get; set; }

        public string ExeFilePath { get; set; }

        public string ExeSecondaryFilePath { get; set; }

        public string ServerArgument { get; set; }

        public string PortArgument { get; set; }

        public string UserArgument { get; set; }

        public string PasswordArgument { get; set; }

        public string IntensityArgument { get; set; }

        public string AlgorithmArgument { get; set; }

        public string BenchmarkArgument { get; set; }

        public string AdditionalArguments { get; set; }

        public string SpeedRegex { get; set; }

        public string ValidShareRegex { get; set; }

        public string InvalidShareRegex { get; set; }

        public string BenchmarkResultRegex { get; set; }

        public bool OmitUrlSchema { get; set; }

        public string MainExecutableName =>
            Path.IsPathRooted(ExeFilePath)
                ? ExeSecondaryFilePath != null && Path.IsPathRooted(ExeSecondaryFilePath) 
                    ? null
                    : ExeSecondaryFilePath
                : ExeFilePath;
    }
}
