using LydShop.Data;
using LydShop.Models;
using LydShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ShopController(ApplicationDbContext db) => _db = db;

        // GET /Shop hoặc /
        public async Task<IActionResult> Index(int? categoryId, string? search,
            string? brand, decimal? minPrice, decimal? maxPrice,
            string? sort, int page = 1)
        {
            int pageSize = 12;

            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Where(p => p.IsActive);

            // Filters
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));

            if (!string.IsNullOrWhiteSpace(brand))
                query = query.Where(p => p.Brand == brand);

            if (minPrice.HasValue)
                query = query.Where(p => (p.SalePrice ?? p.Price) >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => (p.SalePrice ?? p.Price) <= maxPrice.Value);

            // Sort
            query = sort switch
            {
                "price_asc"  => query.OrderBy(p => p.SalePrice ?? p.Price),
                "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.Price),
                "newest"     => query.OrderByDescending(p => p.CreatedAt),
                _            => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
            };

            int totalItems = await query.CountAsync();
            var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Sidebar data
            var categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            var brands = await _db.Products.Where(p => p.IsActive && p.Brand != null)
                                           .Select(p => p.Brand!).Distinct().ToListAsync();
            var featured = await _db.Products.Where(p => p.IsActive && p.IsFeatured)
                                             .Include(p => p.Reviews)
                                             .Take(4).ToListAsync();

            ViewBag.Categories   = categories;
            ViewBag.Brands       = brands;
            ViewBag.Featured     = featured;
            ViewBag.CategoryId   = categoryId;
            ViewBag.Search       = search;
            ViewBag.Brand        = brand;
            ViewBag.MinPrice     = minPrice;
            ViewBag.MaxPrice     = maxPrice;
            ViewBag.Sort         = sort;
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalItems   = totalItems;

            return View(products);
        }

        // GET /Shop/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null) return NotFound();

            var related = await _db.Products
                .Include(p => p.Reviews)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id && p.IsActive)
                .Take(4).ToListAsync();

            bool hasPurchased = false;
            bool hasReviewed  = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                if (userId != null)
                {
                    hasPurchased = await _db.Orders
                        .AnyAsync(o => o.UserId == userId &&
                                       o.Status == OrderStatus.Delivered &&
                                       o.OrderDetails.Any(od => od.ProductId == id));
                    hasReviewed = await _db.Reviews
                        .AnyAsync(r => r.ProductId == id && r.UserId == userId);
                }
            }

            var vm = new ProductDetailViewModel
            {
                Product          = product,
                Reviews          = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList(),
                RelatedProducts  = related,
                UserHasPurchased = hasPurchased,
                UserHasReviewed  = hasReviewed
            };

            return View(vm);
        }
    }
}
