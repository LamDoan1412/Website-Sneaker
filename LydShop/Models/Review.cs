using System.ComponentModel.DataAnnotations;

namespace LydShop.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Range(1, 5)]
        [Display(Name = "Đánh giá")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }

        public bool IsVerifiedPurchase { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
