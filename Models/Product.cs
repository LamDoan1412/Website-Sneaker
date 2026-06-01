using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LydShop.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá khuyến mãi")]
        public decimal? SalePrice { get; set; }

        [Display(Name = "Thương hiệu")]
        public string? Brand { get; set; }

        [Display(Name = "Sizes có sẵn (VD: 36,37,38,39,40)")]
        public string? Sizes { get; set; }

        [Display(Name = "Màu sắc")]
        public string? Colors { get; set; }

        [Display(Name = "Số lượng tồn kho")]
        public int Stock { get; set; } = 0;

        [Display(Name = "Ảnh chính")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Ảnh phụ (JSON)")]
        public string? ImageGallery { get; set; }

        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; } = false;

        [Display(Name = "Còn kinh doanh")]
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Computed
        [NotMapped]
        public decimal CurrentPrice => SalePrice.HasValue && SalePrice.Value > 0 ? SalePrice.Value : Price;

        [NotMapped]
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;

        [NotMapped]
        public List<string> SizeList => string.IsNullOrEmpty(Sizes)
            ? new List<string>()
            : Sizes.Split(',').Select(s => s.Trim()).ToList();
    }
}
