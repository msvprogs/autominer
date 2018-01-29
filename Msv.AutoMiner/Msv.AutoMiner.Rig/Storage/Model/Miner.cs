using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class Miner : IMinerModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int VersionId { get; set; }

        public string Version { get; set; }

        [Required]
        public string FileName { get; set; }

        public string SecondaryFileName { get; set; }

        public bool IsDownloaded { get; set; }

        [Required]
        public string ServerArgument { get; set; }

        public string PortArgument { get; set; }

        public string UserArgument { get; set; }

        public string PasswordArgument { get; set; }

        public string AdditionalArguments { get; set; }

        public string LogFileArgument { get; set; }

        public string IntensityArgument { get; set; }

        public string AlgorithmArgument { get; set; }

        public string DeviceListArgument { get; set; }

        public string DeviceListSeparator { get; set; }

        public string AlternativeServerArgument { get; set; }

        public string AlternativeUserArgument { get; set; }
        
        public string AlternativePasswordArgument { get; set; }

        public bool ReadOutputFromLog { get; set; }

        public string SpeedRegex { get; set; }

        public string ValidShareRegex { get; set; }

        public string InvalidShareRegex { get; set; }

        public string BenchmarkArgument { get; set; }

        public string BenchmarkResultRegex { get; set; }

        public bool OmitUrlSchema { get; set; }

        public virtual ICollection<MinerAlgorithmSetting> AlgorithmValues { get; set; }

        [NotMapped]
        int IMinerModel.MinerId => Id;

        [NotMapped]
        string IMinerModel.MinerName => Name;

        [NotMapped]
        string IMinerModel.ExeFilePath => SecondaryFileName != null || !IsDownloaded
            ? FileName 
            : Path.GetFileName(FileName);

        [NotMapped]
        string IMinerModel.ExeSecondaryFilePath => SecondaryFileName != null && IsDownloaded
            ? Path.GetFileName(SecondaryFileName)
            : SecondaryFileName;
    }
}