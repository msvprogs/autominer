using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class ApiKey
    {
        [Key]
        public string Key { get; set; }

        public ApiKeyType Type { get; set; }

        public DateTime? Expires { get; set; }

        public int? UsagesLeft { get; set; }
    }
}
