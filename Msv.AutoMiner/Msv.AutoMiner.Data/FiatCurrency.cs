using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Data
{
    public class FiatCurrency
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(32)]
        public string Name { get; set; }

        [Required, MaxLength(32)]
        public string Symbol { get; set; }
    }
}
