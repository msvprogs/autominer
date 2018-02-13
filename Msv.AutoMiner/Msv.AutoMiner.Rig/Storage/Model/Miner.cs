using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class Miner : IMinerModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        public int VersionId { get; set; }

        [MaxLength(32)]
        public string Version { get; set; }

        [Required, MaxLength(512)]
        public string FileName { get; set; }

        [MaxLength(512)]
        public string SecondaryFileName { get; set; }

        public bool IsDownloaded { get; set; }

        [Required, MaxLength(32)]
        public string ServerArgument { get; set; }

        [MaxLength(32)]
        public string PortArgument { get; set; }

        [MaxLength(32)]
        public string UserArgument { get; set; }

        [MaxLength(32)]
        public string PasswordArgument { get; set; }

        [MaxLength(128)]
        public string AdditionalArguments { get; set; }

        [MaxLength(32)]
        public string LogFileArgument { get; set; }

        [MaxLength(32)]
        public string IntensityArgument { get; set; }

        [MaxLength(32)]
        public string AlgorithmArgument { get; set; }

        [MaxLength(32)]
        public string DeviceListArgument { get; set; }

        [MaxLength(32)]
        public string DeviceListSeparator { get; set; }

        [MaxLength(32)]
        public string AlternativeServerArgument { get; set; }

        [MaxLength(32)]
        public string AlternativeUserArgument { get; set; }
        
        [MaxLength(32)]
        public string AlternativePasswordArgument { get; set; }

        public bool ReadOutputFromLog { get; set; }

        [MaxLength(256)]
        public string SpeedRegex { get; set; }

        [MaxLength(256)]
        public string ValidShareRegex { get; set; }

        [MaxLength(256)]
        public string InvalidShareRegex { get; set; }

        [MaxLength(32)]
        public string BenchmarkArgument { get; set; }

        [MaxLength(256)]
        public string BenchmarkResultRegex { get; set; }

        [MaxLength(32)]
        public string ApiPortArgument { get; set; }

        public MinerApiType ApiType { get; set; }

        public int? ApiPort { get; set; }

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