using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.FrontEnd.Models.Users;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class UsersController :  EntityControllerBase<User, UserDisplayModel, int>
    {        
        public const string UsersMessageKey = "WalletsMessage";

        private const int SaltLengthBytes = 32;

        private readonly ICryptoRandomGenerator m_CryptoRandom;
        private readonly IPasswordHasher m_PasswordHasher;
        private readonly AutoMinerDbContext m_Context;

        public UsersController(
            ICryptoRandomGenerator cryptoRandom, 
            IPasswordHasher passwordHasher, 
            AutoMinerDbContext context) 
            : base("_UserRowPartial", context)
        {
            m_CryptoRandom = cryptoRandom;
            m_PasswordHasher = passwordHasher;
            m_Context = context;
        }

        public IActionResult Index()
            => View(GetEntityModels(null));

        public IActionResult Create()
            => View("Edit", new UserEditModel());

        public async Task<IActionResult> Edit(int id)
        {
            var user = await m_Context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return NotFound();
            return View(new UserEditModel
            {
                Id = user.Id,
                Name = user.Name,
                Login = user.Login,
                Role = user.Role
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(UserEditModel userModel)
        {
            if (await m_Context.Users.AnyAsync(x => x.Login == userModel.Login && x.Id != userModel.Id))
                ModelState.AddModelError(nameof(userModel.Login), "User with this login already exists");

            var user = await m_Context.Users.FirstOrDefaultAsync(x => x.Id == userModel.Id)
                ?? m_Context.Users.Add(new User()).Entity;
            if (user.PasswordHash.IsNullOrEmpty() && string.IsNullOrEmpty(userModel.Password))
                ModelState.AddModelError(nameof(userModel.Password), "Password isn't filled");

            if (!ModelState.IsValid)
                return View("Edit", userModel);

            user.Name = userModel.Name;
            user.Login = userModel.Login;
            user.Role = userModel.Role;
            if (!string.IsNullOrEmpty(userModel.Password))
            {
                user.Salt = m_CryptoRandom.GenerateRandom(SaltLengthBytes);
                user.PasswordHash = m_PasswordHasher.CalculateHash(userModel.Password, user.Salt);
            }

            await m_Context.SaveChangesAsync();
            TempData[UsersMessageKey] = $"User {user.Login} has been successfully saved";
            return RedirectToAction("Index");
        }

        protected override UserDisplayModel[] GetEntityModels(int[] ids)
        {
            var userQuery = m_Context.Users.AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted);
            if (!ids.IsNullOrEmpty())
                userQuery = userQuery.Where(x => ids.Contains(x.Id));
            return userQuery
                .Select(x => new UserDisplayModel
                {
                    Activity = x.Activity,
                    Id = x.Id,
                    Login = x.Login,
                    Name = x.Name,
                    Role = x.Role
                })
                .ToArray();
        }
    }
}
