using AssetNode.Interface;
    using AssetNode.Models.Entities;
    using Microsoft.EntityFrameworkCore;

namespace AssetNode.Services.Sql
{
    public class AssetDbContext : DbContext
    {

        private readonly ICurrentUserService _currentUser;
        public AssetDbContext(DbContextOptions<AssetDbContext> options, ICurrentUserService currentuserservice=null) : base(options)
        {
            _currentUser = currentuserservice;
        }

        public DbSet<Asset> Assets { get; set; }
        public DbSet<Signal> Signals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<HitoryLog> HitoryLogs { get; set; }

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
                .HasForeignKey(a => a.ParentAssetId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<HitoryLog>()
                  .Property(b => b.ChangedAt)
                  .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<HitoryLog>()
               .Property(h => h.Action)
               .HasConversion<string>();
        }


        //public override int SaveChanges()
        //{
        //    var LogEntries = new List<HitoryLog>();

        //    foreach (var entry in ChangeTracker.Entries())
        //    {
        //        if (entry.Entity is Asset || entry.Entity is Signal)
        //        {
        //            string TableName = entry.Metadata.GetTableName();
        //            int RecordId;


        //            var keyName = entry.Metadata.FindPrimaryKey().Properties.First().Name;
        //            if (entry.State == EntityState.Deleted)
        //            {
        //                RecordId = (int)entry.OriginalValues[keyName];
        //            }
        //            else
        //            {
        //                RecordId = (int)entry.CurrentValues[keyName];
        //            }

        //            if (entry.State == EntityState.Added)
        //            {
        //                LogEntries.Add(new HitoryLog
        //                {
        //                    TableName = TableName,
        //                    RecordId = RecordId,
        //                    Action = ActionType.Add,
        //                    Description = $"{TableName} with ID {RecordId} was added.",
        //                    ChangedBy = _currentUser?.UserName ?? "System",
        //                    //ChangedAt=DateTime.Now,
        //                });
        //            }

        //            else if (entry.State == EntityState.Modified)
        //            {
        //                foreach (var prop in entry.Properties)
        //                {
        //                    if (prop.IsModified)
        //                    {
        //                        LogEntries.Add(new HitoryLog
        //                        {
        //                            TableName = TableName,
        //                            RecordId = RecordId,
        //                            Action = ActionType.Update,
        //                            Description = $"{prop.Metadata.Name} changed from '{prop.OriginalValue}' to '{prop.CurrentValue}'",
        //                            ChangedBy = _currentUser?.UserName ?? "System",
        //                            //ChangedAt = DateTime.Now
        //                        });
        //                    }
        //                }
        //            }
        //            else if (entry.State == EntityState.Deleted)
        //            {
        //                LogEntries.Add(new HitoryLog
        //                {
        //                    TableName = TableName,
        //                    RecordId = RecordId,
        //                    Action = ActionType.Delete,
        //                    Description = $"{RecordId} in table {TableName} Deleted",
        //                    ChangedBy = _currentUser?.UserName ?? "System",
        //                    ChangedAt = DateTime.Now
        //                });
        //            }

        //        }
        //    }

        //    this.AddRange(LogEntries);

        //    return base.SaveChanges();
        //}

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("=== Async SaveChanges Called ===");

            var LogEntries = new List<HitoryLog>();
            var addedEntities = new List<dynamic>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Asset || entry.Entity is Signal)
                {
                    Console.WriteLine($"Processing Entity: {entry.Entity.GetType().Name}, State: {entry.State}");

                    string TableName = entry.Metadata.GetTableName();
                    var keyName = entry.Metadata.FindPrimaryKey().Properties.First().Name;

                    if (entry.State == EntityState.Added)
                    {
                        Console.WriteLine("Found Added Entity - storing for later processing");
                        addedEntities.Add(new
                        {
                            Entry = entry,
                            TableName = TableName,
                            KeyName = keyName
                        });
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        int RecordId = (int)entry.Property(keyName).CurrentValue;
                        foreach (var prop in entry.Properties)
                        {
                            if (prop.IsModified)
                            {
                                LogEntries.Add(new HitoryLog
                                {
                                    TableName = TableName,
                                    RecordId = RecordId,
                                    Action = ActionType.Update,
                                    Description = $"{prop.Metadata.Name} changed from '{prop.OriginalValue}' to '{prop.CurrentValue}'",
                                    ChangedBy = _currentUser?.UserName ?? "System"
                                });
                            }
                        }
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        int RecordId = (int)entry.Property(keyName).OriginalValue;
                        LogEntries.Add(new HitoryLog
                        {
                            TableName = TableName,
                            RecordId = RecordId,
                            Action = ActionType.Delete,
                            Description = $"{RecordId} in table {TableName} Deleted",
                            ChangedBy = _currentUser?.UserName ?? "System",
                            ChangedAt = DateTime.Now
                        });
                    }
                }
            }

            Console.WriteLine($"Added entities found: {addedEntities.Count}");

            var result = await base.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"SaveChanges result: {result}");

            foreach (var addedEntity in addedEntities)
            {
                int RecordId = (int)addedEntity.Entry.Property(addedEntity.KeyName).CurrentValue;
                Console.WriteLine($"Added Entity - Table: {addedEntity.TableName}, Generated ID: {RecordId}");

                LogEntries.Add(new HitoryLog
                {
                    TableName = addedEntity.TableName,
                    RecordId = RecordId,
                    Action = ActionType.Add,
                    Description = $"{addedEntity.TableName} with ID {RecordId} was added.",
                    ChangedBy = _currentUser?.UserName ?? "System"
                });
            }

            Console.WriteLine($"Total log entries to save: {LogEntries.Count}");

            if (LogEntries.Any())
            {
                this.AddRange(LogEntries);
                await base.SaveChangesAsync(cancellationToken);
                Console.WriteLine("History logs saved successfully!");
            }

            return result;
        }


    }
}
