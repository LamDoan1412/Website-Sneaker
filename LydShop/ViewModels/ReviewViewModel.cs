// ViewModels/ReviewViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace LydShop.ViewModels
{
    public class ReviewViewModel
    {
        // Dùng khi submit đánh giá (form)
        [Required]
        public int ProductId { get; set; }

        [Range(1, 5, ErrorMessage = "Vui lòng chọn số sao từ 1 đến 5")]
        [Display(Name = "Đánh giá")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }
    }
}