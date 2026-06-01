// ViewModels/CartViewModel.cs
using LydShop.Models;

namespace LydShop.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();

        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);

        public int TotalQuantity => Items.Sum(i => i.Quantity);

        public decimal ShippingFee => TotalAmount >= 500_000 ? 0 : 30_000;

        public decimal GrandTotal => TotalAmount + ShippingFee;
    }
}