using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.FrontEnd.Models.Authentication;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class AuthenticationController : Controller
    {
        public const string MessageKey = "AuthenticationMessage";

        private readonly IPasswordHasher m_PasswordHasher;
        private readonly AutoMinerDbContext m_Context;

        public AuthenticationController(
            IPasswordHasher passwordHasher,
            AutoMinerDbContext context)
        {
            m_PasswordHasher = passwordHasher;
            m_Context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await m_Context.Users.FirstOrDefaultAsync(x => x.Login == model.Login);
            if (user?.PasswordHash == null 
                || user.Salt == null
                || !user.PasswordHash.SequenceEqual(m_PasswordHasher.CalculateHash(model.Password, user.Salt)))
            {
                ModelState.AddModelError(nameof(model.Login), "Login or password are invalid");
                return View(model);
            }

            var identity = new ClaimsIdentity(
                CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            identity.AddClaims(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Login),
                new Claim(ClaimTypes.Name, user.Name ?? user.Login),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            });

            await HttpContext.SignInAsync(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                });
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            TempData[MessageKey] = "You should be authorized to perform this action";
            return RedirectToAction("Login");
        }
    }
}
