using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Custom_Tournaments_Infrastructure.ViewModels;
using Custom_Tournaments.Domain.Model;
using Custom_Tournaments_Infrastructure;

namespace Custom_Tournaments_Infrastructure.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly Custom_Tournaments_Context _context;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            Custom_Tournaments_Context context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Перевірка чи нікнейм вже зайнятий
            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username.ToLower() == model.Username.ToLower());

            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Цей нікнейм вже зайнятий");
                return View(model);
            }

            // Створюємо Identity користувача
            var appUser = new AppUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(appUser, model.Password);

            if (result.Succeeded)
            {
                // Створюємо запис в нашій таблиці users
                var user = new User
                {
                    IdentityUserId = appUser.Id,
                    Username = model.Username,
                    Email = model.Email,
                    Createdat = DateTime.Now,
                    Updatedat = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(appUser, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Невірний email або пароль");
            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login");

            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdentityUserId == appUser.Id);

            return View(user);
        }
    }
}