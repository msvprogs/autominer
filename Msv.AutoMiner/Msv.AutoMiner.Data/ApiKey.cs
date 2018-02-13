﻿using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class ApiKey
    {
        [Key, MaxLength(128)]
        public string Key { get; set; }

        public ApiKeyType Type { get; set; }

        public DateTime? Expires { get; set; }

        public int? UsagesLeft { get; set; }
    }
}
