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
        [MaxLength(32)]
        public string Version { get; set; }

        [HiddenInput]
        public PlatformType Platform { get; set; }

        public IFormFile ZipFile { get; set; }

        [Required(ErrorMessage = "Executable file name isn't filled")]
        [MaxLength(512)]
        public string ExeFilePath { get; set; }

        [MaxLength(512)]
        public string ExeSecondaryFilePath { get; set; }

        [MaxLength(32)]
        public string ServerArgument { get; set; }

        [MaxLength(32)]
        public string PortArgument { get; set; }

        [MaxLength(32)]
        public string UserArgument { get; set; }

        [MaxLength(32)]
        public string PasswordArgument { get; set; }

        [MaxLength(32)]
        public string IntensityArgument { get; set; }

        [MaxLength(32)]
        public string AlgorithmArgument { get; set; }

        [MaxLength(32)]
        public string BenchmarkArgument { get; set; }

        [MaxLength(32)]
        public string ApiPortArgument { get; set; }

        [MaxLength(32)]
        public string AdditionalArguments { get; set; }

        public MinerApiType MinerApiType { get; set; }

        [Range(-65535, 65535, ErrorMessage = "Invalid port number")]
        public int? MinerApiPort { get; set; }

        [RegexPattern(GroupNames = new[] {"speed"}, ErrorMessage = "Incorrect hashrate regex")]
        [MaxLength(256)]
        public string SpeedRegex { get; set; }

        [RegexPattern(ErrorMessage = "Incorrect valid share regex")]
        [MaxLength(256)]
        public string ValidShareRegex { get; set; }

        [RegexPattern(ErrorMessage = "Incorrect invalid share regex")]
        [MaxLength(256)]
        public string InvalidShareRegex { get; set; }

        [RegexPattern(GroupNames = new[] {"speed"}, ErrorMessage = "Incorrect benchmark hashrate regex")]
        [MaxLength(256)]
        public string BenchmarkResultRegex { get; set; }

        public bool OmitUrlSchema { get; set; }
    }
}
