using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ItemTag> ItemTags { get; set; }
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

            // ItemTag many-to-many
            modelBuilder.Entity<ItemTag>()
                .HasKey(it => new { it.ItemId, it.TagId });

            modelBuilder.Entity<ItemTag>()
                .HasOne(it => it.Item)
                .WithMany(i => i.ItemTags)
                .HasForeignKey(it => it.ItemId);

            modelBuilder.Entity<ItemTag>()
                .HasOne(it => it.Tag)
                .WithMany(t => t.ItemTags)
                .HasForeignKey(it => it.TagId);

            // Lookup FK relationships on Item
            modelBuilder.Entity<Item>()
                .HasOne(i => i.AssetType)
                .WithMany()
                .HasForeignKey(i => i.AssetTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.DomainLookup)
                .WithMany()
                .HasForeignKey(i => i.DomainId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.DivisionLookup)
                .WithMany()
                .HasForeignKey(i => i.DivisionId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.ServiceLineLookup)
                .WithMany()
                .HasForeignKey(i => i.ServiceLineId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.DataSourceLookup)
                .WithMany()
                .HasForeignKey(i => i.DataSourceId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes to speed up lookup/filter queries and joins
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.AssetTypeId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DomainId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DivisionId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ServiceLineId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DataSourceId);

            // Common sorting/filtering columns
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DateAdded);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.Featured);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.PrivacyPhi);

            // LookupValue: queries typically filter by Type and Value
            modelBuilder.Entity<LookupValue>()
                .HasIndex(l => new { l.Type, l.Value });

            // Tags: lookup by Value when creating/attaching tags
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Value);

            // ItemTag many-to-many: primary key exists, add index on TagId for tag->items queries
            modelBuilder.Entity<ItemTag>()
                .HasIndex(it => it.TagId);

            // Users: lookup by principal name
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserPrincipalName);
        }
    }
}
