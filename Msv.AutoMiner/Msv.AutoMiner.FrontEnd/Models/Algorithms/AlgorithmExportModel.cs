using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.FrontEnd.Models.Algorithms
{
    public class AlgorithmExportModel : AlgorithmBaseModel
    {
        [MaxLength(256)]
        public string Aliases { get; set; }
    }
}
