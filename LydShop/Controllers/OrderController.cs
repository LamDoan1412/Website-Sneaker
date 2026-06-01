using LydShop.Data;
using LydShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET /Order/History
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _db.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == user!.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return View(orders);
        }

        // GET /Order/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _db.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST /Order/Cancel/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null) return NotFound();

            if (order.Status != OrderStatus.Pending)
            {
                TempData["Error"] = "Chỉ có thể hủy đơn hàng đang chờ xác nhận!";
                return RedirectToAction("Detail", new { id });
            }

            order.Status    = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã hủy đơn hàng thành công.";
            return RedirectToAction("History");
        }
    }
}
