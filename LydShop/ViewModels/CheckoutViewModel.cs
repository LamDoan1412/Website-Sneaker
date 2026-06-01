// ViewModels/CheckoutViewModel.cs
using System.ComponentModel.DataAnnotations;
using LydShop.Models;

namespace LydShop.ViewModels
{
    public class CheckoutViewModel
    {
        // ── Thông tin giao hàng ──────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string ReceiverPhone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = "";

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        // ── Phương thức thanh toán ───────────────────────
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        // ── Dữ liệu giỏ hàng (không bind từ form) ───────
        public List<CartItem> CartItems { get; set; } = new();

        // ── Tính toán ────────────────────────────────────
        public decimal ItemsTotal => CartItems.Sum(i => i.TotalPrice);
        public decimal ShippingFee => ItemsTotal >= 500_000 ? 0 : 30_000;
        public decimal GrandTotal => ItemsTotal + ShippingFee;
    }
}