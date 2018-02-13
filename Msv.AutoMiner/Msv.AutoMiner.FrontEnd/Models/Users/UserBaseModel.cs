using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Users
{
    public class UserBaseModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required(ErrorMessage = "Login isn't filled")]
        [MaxLength(64)]
        public string Login { get; set; }

        [Required(ErrorMessage = "User name isn't filled")]
        [MaxLength(64)]
        public string Name { get; set; }

        public UserRole Role { get;set; }
    }
}