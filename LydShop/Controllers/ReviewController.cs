using LydShop.Data;
using LydShop.Models;
using LydShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // POST /Review/Submit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ReviewViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin đánh giá.";
                return RedirectToAction("Detail", "Shop", new { id = vm.ProductId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Kiểm tra đã review chưa
            var existed = await _db.Reviews
                .AnyAsync(r => r.ProductId == vm.ProductId && r.UserId == user.Id);
            if (existed)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này rồi!";
                return RedirectToAction("Detail", "Shop", new { id = vm.ProductId });
            }

            // Kiểm tra đã mua chưa
            var hasPurchased = await _db.Orders
                .AnyAsync(o => o.UserId == user.Id &&
                               o.Status == OrderStatus.Delivered &&
                               o.OrderDetails.Any(od => od.ProductId == vm.ProductId));

            var review = new Review
            {
                ProductId         = vm.ProductId,
                UserId            = user.Id,
                Rating            = vm.Rating,
                Comment           = vm.Comment,
                IsVerifiedPurchase = hasPurchased,
                CreatedAt         = DateTime.Now
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction("Detail", "Shop", new { id = vm.ProductId });
        }

        // POST /Review/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user    = await _userManager.GetUserAsync(User);
            var review  = await _db.Reviews.FindAsync(id);

            if (review == null || (review.UserId != user!.Id && !User.IsInRole("Admin")))
                return Forbid();

            int productId = review.ProductId;
            _db.Reviews.Remove(review);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã xóa đánh giá.";
            return RedirectToAction("Detail", "Shop", new { id = productId });
        }
    }
}
