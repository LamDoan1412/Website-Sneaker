using LydShop.Data;
using LydShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var cats = await _db.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.Name).ToListAsync();
            return View(cats);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            if (string.IsNullOrWhiteSpace(category.Slug))
                category.Slug = GenerateSlug(category.Name);

            category.CreatedAt = DateTime.Now;
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Thêm danh mục thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return BadRequest();
            if (!ModelState.IsValid) return View(category);

            if (string.IsNullOrWhiteSpace(category.Slug))
                category.Slug = GenerateSlug(category.Name);

            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật danh mục thành công!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa danh mục!";
            return RedirectToAction("Index");
        }

        private static string GenerateSlug(string name)
        {
            return name.ToLower()
                .Replace("à","a").Replace("á","a").Replace("ả","a").Replace("ã","a").Replace("ạ","a")
                .Replace("ă","a").Replace("ắ","a").Replace("ặ","a").Replace("ằ","a").Replace("ẳ","a").Replace("ẵ","a")
                .Replace("â","a").Replace("ấ","a").Replace("ầ","a").Replace("ẩ","a").Replace("ẫ","a").Replace("ậ","a")
                .Replace("đ","d")
                .Replace("è","e").Replace("é","e").Replace("ẻ","e").Replace("ẽ","e").Replace("ẹ","e")
                .Replace("ê","e").Replace("ế","e").Replace("ề","e").Replace("ể","e").Replace("ễ","e").Replace("ệ","e")
                .Replace("ì","i").Replace("í","i").Replace("ỉ","i").Replace("ĩ","i").Replace("ị","i")
                .Replace("ò","o").Replace("ó","o").Replace("ỏ","o").Replace("õ","o").Replace("ọ","o")
                .Replace("ô","o").Replace("ố","o").Replace("ồ","o").Replace("ổ","o").Replace("ỗ","o").Replace("ộ","o")
                .Replace("ơ","o").Replace("ớ","o").Replace("ờ","o").Replace("ở","o").Replace("ỡ","o").Replace("ợ","o")
                .Replace("ù","u").Replace("ú","u").Replace("ủ","u").Replace("ũ","u").Replace("ụ","u")
                .Replace("ư","u").Replace("ứ","u").Replace("ừ","u").Replace("ử","u").Replace("ữ","u").Replace("ự","u")
                .Replace("ỳ","y").Replace("ý","y").Replace("ỷ","y").Replace("ỹ","y").Replace("ỵ","y")
                .Replace(" ", "-")
                .Replace("--", "-");
        }
    }
}
