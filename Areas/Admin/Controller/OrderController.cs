using LydShop.Data;
using LydShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrderController(ApplicationDbContext db) => _db = db;

        // GET /Admin/Order
        public async Task<IActionResult> Index(OrderStatus? status, string? search, int page = 1)
        {
            int pageSize = 15;
            var query = _db.Orders.Include(o => o.User).AsQueryable();

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.OrderCode.Contains(search) ||
                                         o.ReceiverName.Contains(search) ||
                                         o.ReceiverPhone.Contains(search));

            int total = await query.CountAsync();
            var orders = await query.OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Status      = status;
            ViewBag.Search      = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = (int)Math.Ceiling((double)total / pageSize);

            return View(orders);
        }

        // GET /Admin/Order/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST /Admin/Order/UpdateStatus
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status    = status;
            order.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction("Detail", new { id });
        }
    }
}
