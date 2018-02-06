using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Msv.AutoMiner.FrontEnd.Models.Miners
{
    public class MinerBaseModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name isn't filled")]
        [MaxLength(64)]
        public string Name { get; set; }
    }
}
