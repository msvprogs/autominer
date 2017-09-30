using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.Data
{
    public class TelegramUser
    {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; }
    }
}
