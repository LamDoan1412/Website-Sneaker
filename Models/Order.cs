using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LydShop.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Chờ xác nhận")] Pending = 0,
        [Display(Name = "Đã xác nhận")] Confirmed = 1,
        [Display(Name = "Đang giao hàng")] Shipping = 2,
        [Display(Name = "Đã giao hàng")] Delivered = 3,
        [Display(Name = "Đã hủy")] Cancelled = 4
    }

    public enum PaymentMethod
    {
        [Display(Name = "Thanh toán khi nhận hàng")] COD = 0,
        [Display(Name = "Chuyển khoản ngân hàng")] BankTransfer = 1
    }

    public class Order
    {
        public int Id { get; set; }

        [Display(Name = "Mã đơn hàng")]
        public string OrderCode { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required, Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required, Display(Name = "Số điện thoại")]
        public string ReceiverPhone { get; set; } = string.Empty;

        [Required, Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Phí vận chuyển")]
        public decimal ShippingFee { get; set; } = 30000;

        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
