using LydShop.Data;
using LydShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DashboardController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            ViewBag.TotalProducts  = await _db.Products.CountAsync(p => p.IsActive);
            ViewBag.TotalCategories= await _db.Categories.CountAsync(c => c.IsActive);
            ViewBag.TotalOrders    = await _db.Orders.CountAsync();
            ViewBag.TotalUsers     = await _db.Users.CountAsync();

            ViewBag.PendingOrders  = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            ViewBag.MonthRevenue   = await _db.Orders
                .Where(o => o.CreatedAt >= startOfMonth && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.TotalRevenue   = await _db.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Recent orders
            ViewBag.RecentOrders = await _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(8).ToListAsync();

            // Top products
            ViewBag.TopProducts = await _db.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => new { od.ProductId, od.ProductName })
                .Select(g => new { g.Key.ProductName, TotalSold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(5).ToListAsync();

            // Revenue by status
            ViewBag.OrdersByStatus = await _db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return View();
        }
    }
}
