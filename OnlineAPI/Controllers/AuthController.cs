using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnlineAPI.DTOs;
using System.Security.Claims;


namespace OnlineAPI.Controllers
{ 
    public class AuthController : Controller
    {
        private readonly AppContext _context;
        public AuthController(AppContext db)
        {
            _context = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Tasks");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Entities.LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Простая проверка пользователя (в реальном приложении используйте хеширование паролей!)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Tasks");
            }

            ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
   
}
