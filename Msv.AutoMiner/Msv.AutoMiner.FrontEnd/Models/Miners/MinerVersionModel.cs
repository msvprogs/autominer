using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.FrontEnd.Infrastructure;

namespace Msv.AutoMiner.FrontEnd.Models.Miners
{
    public class MinerVersionModel
    {
        [HiddenInput]
        public int Id { get; set; }

        public string MinerName { get; set; }

        [HiddenInput]
        public int MinerId { get; set; }

        [Required(ErrorMessage = "Version isn't filled")]
        public string Version { get; set; }

        [HiddenInput]
        public PlatformType Platform { get; set; }

        public IFormFile ZipFile { get; set; }

        [Required(ErrorMessage = "Executable file name isn't filled")]
        public string ExeFilePath { get; set; }

        public string ExeSecondaryFilePath { get; set; }

        public string ServerArgument { get; set; }

        public string PortArgument { get; set; }

        public string UserArgument { get; set; }

        public string PasswordArgument { get; set; }

        public string IntensityArgument { get; set; }

        public string AlgorithmArgument { get; set; }

        public string BenchmarkArgument { get; set; }

        public string ApiPortArgument { get; set; }

        public string AdditionalArguments { get; set; }

        public MinerApiType MinerApiType { get; set; }

        [Range(-65535, 65535, ErrorMessage = "Invalid port number")]
        public int? MinerApiPort { get; set; }

        [RegexPattern(GroupNames = new[] {"speed"}, ErrorMessage = "Incorrect hashrate regex")]
        public string SpeedRegex { get; set; }

        [RegexPattern(ErrorMessage = "Incorrect valid share regex")]
        public string ValidShareRegex { get; set; }

        [RegexPattern(ErrorMessage = "Incorrect invalid share regex")]
        public string InvalidShareRegex { get; set; }

        [RegexPattern(GroupNames = new[] {"speed"}, ErrorMessage = "Incorrect benchmark hashrate regex")]
        public string BenchmarkResultRegex { get; set; }

        public bool OmitUrlSchema { get; set; }
    }
}
