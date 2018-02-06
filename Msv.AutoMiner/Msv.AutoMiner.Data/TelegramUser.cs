using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Data
{
    public class TelegramUser
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(64)]
        public string UserName { get; set; }
    }
}
