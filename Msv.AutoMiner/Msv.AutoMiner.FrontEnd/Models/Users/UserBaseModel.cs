using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Users
{
    public class UserBaseModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required(ErrorMessage = "Login isn't filled")]
        public string Login { get; set; }

        [Required(ErrorMessage = "User name isn't filled")]
        public string Name { get; set; }

        public UserRole Role { get;set; }
    }
}