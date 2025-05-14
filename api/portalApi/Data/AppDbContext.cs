using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<LookupValue> LookupValues { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserAssetOpenHistory> UserAssetOpenHistories { get; set; }
        public DbSet<UserSearchHistory> UserSearchHistories { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserFavorite>()
                .HasKey(uf => new { uf.UserId, uf.ItemId });

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(uf => uf.UserId);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Item)
                .WithMany()
                .HasForeignKey(uf => uf.ItemId);
        }
    }
}
