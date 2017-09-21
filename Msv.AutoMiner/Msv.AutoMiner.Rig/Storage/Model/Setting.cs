﻿using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Rig.Storage.Model
{
    public class Setting
    {
        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
