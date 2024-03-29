﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Msv.AutoMiner.FrontEnd.Models.Rigs
{
    public class RigBaseModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required(ErrorMessage = "Rig name is required")]
        [MaxLength(64)]
        public string Name { get; set; }
    }
}
