using LydShop.Data;
using LydShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET /Admin/Product
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            int pageSize = 15;
            var query = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            int total = await query.CountAsync();
            var products = await query.OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Search      = search;
            ViewBag.CategoryId  = categoryId;
            ViewBag.Categories  = await _db.Categories.ToListAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = (int)Math.Ceiling((double)total / pageSize);

            return View(products);
        }

        // GET /Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST /Admin/Product/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (imageFile != null)
                product.ImageUrl = await SaveImageAsync(imageFile);

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name");
                return View(product);
            }

            product.CreatedAt = DateTime.Now;
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // GET /Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST /Admin/Product/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return BadRequest();

            if (imageFile != null)
                product.ImageUrl = await SaveImageAsync(imageFile);

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _db.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
                return View(product);
            }

            product.UpdatedAt = DateTime.Now;
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        // GET /Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST /Admin/Product/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Xóa ảnh nếu có
            if (!string.IsNullOrEmpty(product.ImageUrl))
                DeleteImage(product.ImageUrl);

            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa sản phẩm!";
            return RedirectToAction("Index");
        }

        // Toggle Active
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.IsActive = !product.IsActive;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploads = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploads);
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(uploads, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return "/images/products/" + fileName;
        }

        private void DeleteImage(string imageUrl)
        {
            var path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
