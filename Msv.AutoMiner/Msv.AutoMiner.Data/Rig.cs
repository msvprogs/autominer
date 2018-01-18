using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class Rig
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string RegistrationPassword { get; set; }

        public DateTime Created { get; set; }

        public bool IsActive { get; set; }

        public byte[] ClientCertificateSerial { get; set; }

        public byte[] ClientCertificateThumbprint { get; set; }

        public ValueAggregationType DifficultyAggregationType { get; set; }

        public ValueAggregationType PriceAggregationType { get; set; }
    }
}
