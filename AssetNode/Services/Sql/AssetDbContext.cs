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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .HasOne(e => e.ParentAsset)
                .WithMany(a => a.Children)
                .HasForeignKey(a => a.ParentAssetId);
        }
    }
}
