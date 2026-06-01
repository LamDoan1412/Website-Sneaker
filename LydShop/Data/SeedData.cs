using LydShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LydShop.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            // ── Roles ──────────────────────────────────────────────────────────
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── Admin account ─────────────────────────────────────────────────
            var adminEmail = "lam@gmail.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    IsActive = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ── Categories ────────────────────────────────────────────────────
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Giày Thể Thao",   Slug = "giay-the-thao",   Description = "Giày thể thao năng động", ImageUrl = "/images/cat_sport.jpg",    IsActive = true },
                    new Category { Name = "Giày Chạy Bộ",    Slug = "giay-chay-bo",    Description = "Chuyên dùng cho chạy bộ", ImageUrl = "/images/cat_running.jpg",  IsActive = true },
                    new Category { Name = "Giày Thời Trang",  Slug = "giay-thoi-trang", Description = "Phong cách thời trang",   ImageUrl = "/images/cat_fashion.jpg",  IsActive = true },
                    new Category { Name = "Giày Sandal",      Slug = "giay-sandal",     Description = "Sandal nam nữ",           ImageUrl = "/images/cat_sandal.jpg",   IsActive = true },
                    new Category { Name = "Giày Boots",       Slug = "giay-boots",      Description = "Boots cổ cao thời trang", ImageUrl = "/images/cat_boots.jpg",    IsActive = true },
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // ── Products ──────────────────────────────────────────────────────
            if (!context.Products.Any())
            {
                var catSport   = context.Categories.First(c => c.Slug == "giay-the-thao");
                var catRunning = context.Categories.First(c => c.Slug == "giay-chay-bo");
                var catFashion = context.Categories.First(c => c.Slug == "giay-thoi-trang");
                var catSandal  = context.Categories.First(c => c.Slug == "giay-sandal");
                var catBoots   = context.Categories.First(c => c.Slug == "giay-boots");

                var products = new List<Product>
                {
                    new Product { Name = "Nike Air Max 270",        Brand = "Nike",    Price = 3200000, SalePrice = 2800000, CategoryId = catSport.Id,   Sizes = "38,39,40,41,42,43", Colors = "Đen,Trắng,Đỏ",    Stock = 50, IsFeatured = true,  IsActive = true, ImageUrl = "/images/products/af1-07.jpg",   Description = "Giày thể thao Nike Air Max 270 đệm khí siêu êm ái." },
                    new Product { Name = "Adidas Ultraboost 22",    Brand = "Adidas",  Price = 3500000, SalePrice = 3000000, CategoryId = catRunning.Id, Sizes = "38,39,40,41,42,43", Colors = "Trắng,Xanh,Xám", Stock = 40, IsFeatured = true,  IsActive = true, ImageUrl = "/images/products/adidas-ultraboost.jpg", Description = "Giày chạy bộ Adidas Ultraboost công nghệ Boost tiên tiến." },
                    new Product { Name = "Converse Chuck Taylor",   Brand = "Converse",Price = 1200000, SalePrice = null,    CategoryId = catFashion.Id, Sizes = "36,37,38,39,40,41,42", Colors = "Đen,Trắng,Đỏ",Stock = 60, IsFeatured = true,  IsActive = true, ImageUrl = "/images/products/converse-chuck.jpg",    Description = "Converse Chuck Taylor All Star classic mọi thời đại." },
                    new Product { Name = "Vans Old Skool",          Brand = "Vans",    Price = 1500000, SalePrice = 1300000, CategoryId = catFashion.Id, Sizes = "36,37,38,39,40,41,42", Colors = "Đen/Trắng,Navy",Stock = 45, IsFeatured = false, IsActive = true, ImageUrl = "/images/products/vans-oldskool.jpg",     Description = "Vans Old Skool biểu tượng thời trang đường phố." },
                    new Product { Name = "Nike React Infinity Run",  Brand = "Nike",    Price = 2900000, SalePrice = 2500000, CategoryId = catRunning.Id, Sizes = "39,40,41,42,43",    Colors = "Trắng,Đen",      Stock = 30, IsFeatured = true,  IsActive = true, ImageUrl = "/images/products/nike-react.jpg",         Description = "Giày chạy bộ Nike React Infinity giảm chấn tối ưu." },
                    new Product { Name = "Adidas Stan Smith",       Brand = "Adidas",  Price = 1800000, SalePrice = null,    CategoryId = catFashion.Id, Sizes = "36,37,38,39,40,41,42,43", Colors = "Trắng/Xanh lá",Stock = 55, IsFeatured = false,IsActive = true, ImageUrl = "/images/products/adidas-stansmith.jpg",  Description = "Adidas Stan Smith – biểu tượng sneaker thanh lịch." },
                    new Product { Name = "Birkenstock Arizona",     Brand = "Birkenstock", Price = 2200000, SalePrice = 1900000, CategoryId = catSandal.Id, Sizes = "36,37,38,39,40,41,42", Colors = "Nâu,Đen,Beige", Stock = 35, IsFeatured = false, IsActive = true, ImageUrl = "/images/products/birkenstock.jpg",    Description = "Sandal Birkenstock Arizona thoải mái và bền bỉ." },
                    new Product { Name = "Dr. Martens 1460",        Brand = "Dr. Martens", Price = 4500000, SalePrice = 4000000, CategoryId = catBoots.Id, Sizes = "37,38,39,40,41,42,43", Colors = "Đen,Cherry Red", Stock = 25, IsFeatured = true,  IsActive = true, ImageUrl = "/images/products/drmartens.jpg",      Description = "Boots Dr. Martens 1460 cổ điển không bao giờ lỗi mốt." },
                    new Product { Name = "Puma RS-X",               Brand = "Puma",    Price = 2000000, SalePrice = 1700000, CategoryId = catSport.Id,   Sizes = "38,39,40,41,42",    Colors = "Trắng/Xám,Xanh",Stock = 40, IsFeatured = false, IsActive = true, ImageUrl = "/images/products/puma-rsx.jpg",           Description = "Puma RS-X phong cách retro đầy cá tính." },
                    new Product { Name = "New Balance 574",         Brand = "New Balance", Price = 2400000, SalePrice = 2100000, CategoryId = catSport.Id, Sizes = "38,39,40,41,42,43", Colors = "Xám,Navy,Xanh lá",Stock = 38, IsFeatured = false,IsActive = true, ImageUrl = "/images/products/newbalance574.jpg", Description = "New Balance 574 – Kinh điển, thoải mái, đa năng." },
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
