using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("miners")]
    public class Miner
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(512)]
        public string FileName { get; set; }

        [MaxLength(512)]
        public string SecondaryFileName { get; set; }

        [Required]
        [MaxLength(255)]
        public string ServerArgument { get; set; }

        [MaxLength(255)]
        public string PortArgument { get; set; }

        [Required]
        [MaxLength(255)]
        public string UserArgument { get; set; }

        [MaxLength(255)]
        public string PasswordArgument { get; set; }

        [MaxLength(512)]
        public string OtherArguments { get; set; }

        [MaxLength(255)]
        public string LogFileArgument { get; set; }

        [MaxLength(255)]
        public string IntensityArgument { get; set; }

        [MaxLength(255)]
        public string DifficultyMultiplierArgument { get; set; }

        [MaxLength(255)]
        public string AlgorithmArgument { get; set; }

        [MaxLength(255)]
        public string AlternativeServerArgument { get; set; }

        [MaxLength(255)]
        public string AlternativeUserArgument { get; set; }

        [MaxLength(255)]
        public string AlternativePasswordArgument { get; set; }

        public bool ReadOutputFromLog { get; set; }

        [MaxLength(512)]
        public string SpeedRegex { get; set; }

        [MaxLength(512)]
        public string ValidShareRegex { get; set; }

        public bool OmitUrlSchema { get; set; }

        public virtual ICollection<Pool> Pools { get; set; }

        public virtual ICollection<MinerAlgorithmValue> AlgorithmValues { get; set; }
    }
}

