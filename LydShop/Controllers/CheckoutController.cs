using LydShop.Data;
using LydShop.Helpers;
using LydShop.Models;
using LydShop.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private const string CartKey = "ShoppingCart";
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET /Checkout
        public async Task<IActionResult> Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartKey) ?? new();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống, vui lòng thêm sản phẩm!";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var vm = new CheckoutViewModel
            {
                CartItems      = cart,
                ReceiverName   = user?.FullName ?? "",
                ReceiverPhone  = user?.Phone ?? "",
                ShippingAddress= user?.Address ?? ""
            };
            return View(vm);
        }

        // POST /Checkout/PlaceOrder
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel vm)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartKey) ?? new();
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            vm.CartItems = cart;

            if (!ModelState.IsValid)
                return View("Index", vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Tạo mã đơn hàng
            var orderCode = "LYD" + DateTime.Now.ToString("yyyyMMddHHmmss") +
                            new Random().Next(100, 999);

            var order = new Order
            {
                OrderCode       = orderCode,
                UserId          = user.Id,
                ReceiverName    = vm.ReceiverName,
                ReceiverPhone   = vm.ReceiverPhone,
                ShippingAddress = vm.ShippingAddress,
                Note            = vm.Note,
                PaymentMethod   = vm.PaymentMethod,
                ShippingFee     = 30000,
                TotalAmount     = cart.Sum(i => i.TotalPrice) + 30000,
                Status          = OrderStatus.Pending,
                CreatedAt       = DateTime.Now,
                OrderDetails    = cart.Select(i => new OrderDetail
                {
                    ProductId    = i.ProductId,
                    ProductName  = i.ProductName,
                    ProductImage = i.ProductImage,
                    Size         = i.Size,
                    Color        = i.Color,
                    Quantity     = i.Quantity,
                    UnitPrice    = i.UnitPrice
                }).ToList()
            };

            _db.Orders.Add(order);

            // Trừ tồn kho
            foreach (var item in cart)
            {
                var product = await _db.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.Stock = Math.Max(0, product.Stock - item.Quantity);
            }

            await _db.SaveChangesAsync();
            HttpContext.Session.Remove(CartKey);

            TempData["Success"] = $"Đặt hàng thành công! Mã đơn: {orderCode}";
            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        // GET /Checkout/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null) return NotFound();
            return View(order);
        }
    }
}
