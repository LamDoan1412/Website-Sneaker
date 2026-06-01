using LydShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        // GET /Admin/User
        public async Task<IActionResult> Index(string? search)
        {
            var users = await _userManager.Users.ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
                users = users.Where(u => u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                          (u.Email != null && u.Email.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();

            ViewBag.Search = search;
            return View(users);
        }

        // GET /Admin/User/Detail/id
        public async Task<IActionResult> Detail(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            return View(user);
        }

        // POST /Admin/User/ToggleActive
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = user.IsActive ? "Đã kích hoạt tài khoản." : "Đã khóa tài khoản.";
            return RedirectToAction("Index");
        }

        // POST /Admin/User/MakeAdmin
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                await _userManager.AddToRoleAsync(user, "Admin");

            TempData["Success"] = "Đã cấp quyền Admin cho người dùng.";
            return RedirectToAction("Detail", new { id });
        }
    }
}
