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
        public DbSet<Owner> Owners { get; set; }
        public DbSet<ImportJob> ImportJobs { get; set; }
        // Removed: public DbSet<ItemDataConsumer> ItemDataConsumers { get; set; }

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

            // Removed ServiceLine; use OperatingEntity instead

            modelBuilder.Entity<Item>()
                .HasOne(i => i.DataSourceLookup)
                .WithMany()
                .HasForeignKey(i => i.DataSourceId)
                .OnDelete(DeleteBehavior.NoAction);

            // Item -> OperatingEntity (LookupValue Type="OperatingEntity")
            modelBuilder.Entity<Item>()
                .HasOne(i => i.OperatingEntityLookup)
                .WithMany()
                .HasForeignKey(i => i.OperatingEntityId)
                .OnDelete(DeleteBehavior.NoAction);

            // Item -> Status (LookupValue Type="Status")
            modelBuilder.Entity<Item>()
                .HasOne(i => i.StatusLookup)
                .WithMany()
                .HasForeignKey(i => i.StatusId)
                .OnDelete(DeleteBehavior.NoAction);

            // Item -> Owner (separate entity); optional
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Owner)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Item -> ExecutiveSponsor (Owner)
            modelBuilder.Entity<Item>()
                .HasOne(i => i.ExecutiveSponsor)
                .WithMany()
                .HasForeignKey(i => i.ExecutiveSponsorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Item -> RefreshFrequency (LookupValue Type="RefreshFrequency")
            modelBuilder.Entity<Item>()
                .HasOne(i => i.RefreshFrequencyLookup)
                .WithMany()
                .HasForeignKey(i => i.RefreshFrequencyId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes to speed up lookup/filter queries and joins
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.AssetTypeId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DomainId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DivisionId);

            // Removed ServiceLine index

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DataSourceId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.StatusId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.OwnerId);

            // Common sorting/filtering columns
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DateAdded);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.Featured);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.PrivacyPhi);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.PrivacyPii);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.HasRls);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.OperatingEntityId);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.RefreshFrequencyId);

            // LookupValue: queries typically filter by Type and Value
            modelBuilder.Entity<LookupValue>()
                .HasIndex(l => new { l.Type, l.Value });

            // Tags: lookup by Value when creating/attaching tags
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Value);

            // ItemTag many-to-many: primary key exists, add index on TagId for tag->items queries
            modelBuilder.Entity<ItemTag>()
                .HasIndex(it => it.TagId);

            // Removed ItemDataConsumer many-to-many. DataConsumers stored as free-text.

            // New optional lookup FKs
            modelBuilder.Entity<Item>()
                .HasOne(i => i.PotentialToConsolidate)
                .WithMany()
                .HasForeignKey(i => i.PotentialToConsolidateId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.PotentialToAutomate)
                .WithMany()
                .HasForeignKey(i => i.PotentialToAutomateId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.SponsorBusinessValue)
                .WithMany()
                .HasForeignKey(i => i.SponsorBusinessValueId)
                .OnDelete(DeleteBehavior.NoAction);

            // MustDo2025 removed

            modelBuilder.Entity<Item>()
                .HasOne(i => i.DevelopmentEffortLookup)
                .WithMany()
                .HasForeignKey(i => i.DevelopmentEffortId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.EstimatedDevHoursLookup)
                .WithMany()
                .HasForeignKey(i => i.EstimatedDevHoursId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.ResourcesDevelopmentLookup)
                .WithMany()
                .HasForeignKey(i => i.ResourcesDevelopmentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.ResourcesAnalystsLookup)
                .WithMany()
                .HasForeignKey(i => i.ResourcesAnalystsId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.ResourcesPlatformLookup)
                .WithMany()
                .HasForeignKey(i => i.ResourcesPlatformId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.ResourcesDataEngineeringLookup)
                .WithMany()
                .HasForeignKey(i => i.ResourcesDataEngineeringId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.PotentialToConsolidateId);
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.PotentialToAutomateId);
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.SponsorBusinessValueId);
            // MustDo2025 index removed
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DevelopmentEffortId);
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.EstimatedDevHoursId);
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ResourcesDevelopmentId);
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ResourcesAnalystsId);
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ResourcesPlatformId);
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ResourcesDataEngineeringId);

            // Product Impact Category
            modelBuilder.Entity<Item>()
                .HasOne(i => i.ProductImpactCategory)
                .WithMany()
                .HasForeignKey(i => i.ProductImpactCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.ProductImpactCategoryId);

            // Users: lookup by principal name (unique)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserPrincipalName)
                .IsUnique();

            // ImportJob: token unique for lookup
            modelBuilder.Entity<ImportJob>()
                .HasIndex(j => j.Token)
                .IsUnique();
        }
    }
}
