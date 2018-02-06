using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class Rig : IEntity<int>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(32)]
        public string RegistrationPassword { get; set; }

        public DateTime Created { get; set; }

        [MaxLength(256)]
        public byte[] ClientCertificateSerial { get; set; }

        [MaxLength(256)]
        public byte[] ClientCertificateThumbprint { get; set; }

        public ValueAggregationType DifficultyAggregationType { get; set; }

        public ValueAggregationType PriceAggregationType { get; set; }

        public ActivityState Activity { get; set; }
    }
}
