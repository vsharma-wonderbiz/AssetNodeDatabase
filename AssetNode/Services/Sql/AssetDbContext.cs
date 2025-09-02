    using Microsoft.EntityFrameworkCore;
    using AssetNode.Models.Entities;

    namespace AssetNode.Services.Sql
    {
        public class AssetDbContext: DbContext
        {
            public AssetDbContext(DbContextOptions<AssetDbContext> options) : base(options)
            {

            }

            public DbSet<Asset> Assets { get; set; }
            public DbSet<Signal> Signals { get; set; }
            public DbSet<User> Users { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Asset>()
                     .HasIndex(a => a.Name)
                     .IsUnique();

            modelBuilder.Entity<Signal>()
                .HasIndex(a => a.SignalName)
                .IsUnique();


                modelBuilder.Entity<Asset>()
                    .HasOne(e => e.ParentAsset)
                    .WithMany(a => a.Children)
                    .HasForeignKey(a => a.ParentAssetId);

                modelBuilder.Entity<Asset>()
                    .HasMany(e => e.Signals)
                    .WithOne(e => e.asset)
                    .HasForeignKey(a => a.AssetID);


                modelBuilder.Entity<User>()
                 .HasIndex(u => u.Username)
                 .IsUnique();

                modelBuilder.Entity<User>()
                   .HasIndex(u => u.Email)
                   .IsUnique();
        }
        }
    }
