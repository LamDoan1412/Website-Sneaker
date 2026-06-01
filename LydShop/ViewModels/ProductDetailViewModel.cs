// ViewModels/ProductDetailViewModel.cs
using LydShop.Models;

namespace LydShop.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;

        public List<Review> Reviews { get; set; } = new();

        public List<Product> RelatedProducts { get; set; } = new();

        public bool UserHasPurchased { get; set; }

        public bool UserHasReviewed { get; set; }

        public double AverageRating =>
            Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;

        public int ReviewCount => Reviews.Count;
    }
}