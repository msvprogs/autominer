using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class Exchange : IEntity<ExchangeType>
    {
        [Key]
        public ExchangeType Type { get; set; }

        public ActivityState Activity { get; set; }

        [MaxLength(256)]
        public string PublicKey { get; set; }

        [MaxLength(256)]
        public string PrivateKey { get; set; }

        ExchangeType IEntity<ExchangeType>.Id
        {
            get => Type;
            set => Type = value;
        }
    }
}