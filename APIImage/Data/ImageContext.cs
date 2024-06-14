using Microsoft.EntityFrameworkCore;
using APIImage.Models;
using APIImage.Models.Domain;

namespace APIImage.Data
{
    public class ImageContext : DbContext
    {
        public ImageContext(DbContextOptions<ImageContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<History> Histories { get; set; } 
    }
}
