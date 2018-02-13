using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class User : IEntity<int>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ActivityState Activity { get; set; }

        [Required, MaxLength(64)]
        public string Login { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        [MaxLength(32)]
        public byte[] PasswordHash { get; set; }

        [MaxLength(32)]
        public byte[] Salt { get; set; }

        public UserRole Role { get; set; }
    }
}
