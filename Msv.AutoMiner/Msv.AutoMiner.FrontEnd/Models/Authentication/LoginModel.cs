using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.FrontEnd.Models.Authentication
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Login is not filled")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Password is not filled")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
