using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.FrontEnd.Models.Users
{
    public class UserEditModel : UserBaseModel
    {
        [DataType(DataType.Password)]
        [MaxLength(256)]
        public string Password { get; set; }
    }
}