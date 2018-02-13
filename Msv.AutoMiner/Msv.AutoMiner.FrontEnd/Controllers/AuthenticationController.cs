using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
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
                || !user.PasswordHash.SequenceEqual(m_PasswordHasher.CalculateHash(model.Password, user.Salt))
                || user.Activity != ActivityState.Active)
            {
                ModelState.AddModelError(nameof(model.Login), "Login or password are invalid");
                return View(model);
            }

            await m_Context.UserLogins.AddAsync(new UserLogin
            {
                DateTime = DateTime.UtcNow,
                UserId = user.Id,
                RemoteAddress = HttpContext.Connection.RemoteIpAddress.ToString()
            });
            await m_Context.SaveChangesAsync();

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
