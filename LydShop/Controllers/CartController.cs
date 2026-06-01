using LydShop.Data;
using LydShop.Helpers;
using LydShop.Models;
using LydShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Controllers
{
    public class CartController : Controller
    {
        private const string CartKey = "ShoppingCart";
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db) => _db = db;

        private List<CartItem> GetCart() =>
            HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartKey) ?? new List<CartItem>();

        private void SaveCart(List<CartItem> cart) =>
            HttpContext.Session.SetObjectAsJson(CartKey, cart);

        // GET /Cart
        public IActionResult Index()
        {
            var cart = GetCart();
            var vm = new CartViewModel { Items = cart };
            return View(vm);
        }

        // POST /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1,
            string? size = null, string? color = null)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.Size == size &&
                i.Color == color);

            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem
                {
                    ProductId    = product.Id,
                    ProductName  = product.Name,
                    ProductImage = product.ImageUrl,
                    Size         = size,
                    Color        = color,
                    Quantity     = quantity,
                    UnitPrice    = product.CurrentPrice
                });

            SaveCart(cart);

            TempData["Success"] = $"Đã thêm \"{product.Name}\" vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        // POST /Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, string? size, string? color, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.Size == size &&
                i.Color == color);

            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // POST /Cart/Remove
        [HttpPost]
        public IActionResult Remove(int productId, string? size, string? color)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i =>
                i.ProductId == productId &&
                i.Size == size &&
                i.Color == color);

            if (item != null) cart.Remove(item);

            SaveCart(cart);
            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        // POST /Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartKey);
            return RedirectToAction("Index");
        }

        // GET /Cart/Count (AJAX)
        public IActionResult Count()
        {
            var cart = GetCart();
            return Json(cart.Sum(i => i.Quantity));
        }
    }
}
