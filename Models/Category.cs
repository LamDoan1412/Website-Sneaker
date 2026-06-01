using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LydShop.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Slug URL")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "Ảnh đại diện")]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
