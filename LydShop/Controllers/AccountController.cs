using LydShop.Data;
using LydShop.Models;
using LydShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
        {
            _userManager  = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Shop");
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(vm.Email);
                if (user != null && !user.IsActive)
                {
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError("", "Tài khoản đã bị khóa. Vui lòng liên hệ admin.");
                    return View(vm);
                }

                if (await _userManager.IsInRoleAsync(user!, "Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                return RedirectToAction("Index", "Shop");
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View(vm);
        }

        // GET /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Shop");
            return View();
        }

        // POST /Account/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName     = vm.Email,
                Email        = vm.Email,
                FullName     = vm.FullName,
                Phone        = vm.Phone,
                IsActive     = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với LydShop.";
                return RedirectToAction("Index", "Shop");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(vm);
        }

        // POST /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Shop");
        }

        // GET /Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST /Account/Profile
        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string fullName, string? phone, string? address)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = fullName;
            user.Phone    = phone;
            user.Address  = address;

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }

        // GET /Account/AccessDenied
        public IActionResult AccessDenied() => View();
    }
}
