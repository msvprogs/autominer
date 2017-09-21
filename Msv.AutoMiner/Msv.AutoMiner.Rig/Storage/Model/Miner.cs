using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class Miner
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string FileName { get; set; }

        public string SecondaryFileName { get; set; }

        [Required]
        public string ServerArgument { get; set; }

        public string PortArgument { get; set; }

        public string UserArgument { get; set; }

        public string PasswordArgument { get; set; }

        public string AdditionalArguments { get; set; }

        public string LogFileArgument { get; set; }

        public string IntensityArgument { get; set; }

        public string AlgorithmArgument { get; set; }

        public string AlternativeServerArgument { get; set; }

        public string AlternativeUserArgument { get; set; }
        
        public string AlternativePasswordArgument { get; set; }

        public bool ReadOutputFromLog { get; set; }

        public string SpeedRegex { get; set; }

        public string ValidShareRegex { get; set; }

        public bool OmitUrlSchema { get; set; }

        public virtual ICollection<MinerAlgorithmSetting> AlgorithmValues { get; set; }
    }
}