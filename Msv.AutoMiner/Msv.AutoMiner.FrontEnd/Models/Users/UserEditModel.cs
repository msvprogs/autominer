using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.FrontEnd.Models.Users
{
    public class UserEditModel : UserBaseModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}