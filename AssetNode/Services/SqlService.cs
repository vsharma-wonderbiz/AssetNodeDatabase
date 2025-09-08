using AssetNode.Interface;
using AssetNode.Services.Sql;
using AssetNode.Models.Entities;
using Microsoft.EntityFrameworkCore;
using AssetNode.Models.Dtos;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Internal;
using AssetNode.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace AssetNode.Services
{
    public class SqlService :ISqlInterface
    {
        private readonly IDbContextFactory<AssetDbContext> _dbfactory;
        private readonly AssetDbContext _db;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ICurrentUserService _currentUser;

        public SqlService(AssetDbContext db, IDbContextFactory<AssetDbContext> dbfactory, IHubContext<NotificationHub> hubContext, ICurrentUserService currentuserservice)
        {
            _db = db;
            _dbfactory = dbfactory;
            _hubContext = hubContext;
            _currentUser = currentuserservice;
        }
        
        public async Task<List<AssetNodes>> GetJsonHierarchy()
        {
            using var db = _dbfactory.CreateDbContext();
            var allAssets = await db.Assets.ToListAsync();
            var AllSignals = await db.Signals.ToListAsync();

            var displaydata = allAssets.Select(x => new AssetNodes
            {
                Id = x.Id,
                Name = x.Name,
                ParentAssetId = x.ParentAssetId,
                Children = new List<AssetNodes>(),
                Signals = new List<SignalNodeDto>()
            }).ToList();

            var displaySignals = AllSignals.Select(x => new SignalNodeDto
            {
                SignalId = x.SignalId,
                SignalName = x.SignalName,
                ValueType = x.ValueType,
                Description = x.Description,
                AssetID = x.AssetID
            }).ToList();

          
            var lookup = displaydata.ToLookup(x => x.ParentAssetId);

           
            foreach (var node in displaydata)
            {
                node.Children = lookup[node.Id].ToList();
            }
            var tree = lookup[null].ToList();

            //========AddingNewEventArgs signals to the assets================

            var SignalLookup = displaySignals.ToLookup(x => x.AssetID);

            foreach (var node in displaydata)
            {
                if (SignalLookup.Contains(node.Id))
                {
                    node.Signals.AddRange(SignalLookup[node.Id]);
;                }
            }
            return tree;
        }

       


        public async Task<Asset> AddAsset(SqladdAsset dto)
        {
            int? parentId = dto.ParentAssetId;

            
            if (parentId == 0 || parentId == null)
                parentId = null;

            if (parentId != null)
            {
                
                bool childExists = await _db.Assets
                    .AnyAsync(a => a.ParentAssetId == parentId && a.Name == dto.Name);
                bool samePatentAsset = await _db.Assets.AnyAsync(a => a.Name == dto.Name);
                bool IdExist = await _db.Assets.AnyAsync(a => a.Id == parentId);
                if (!IdExist)
                {
                    throw new InvalidOperationException("No Such Id Exist");
                }
                if (childExists)
                {
                    throw new InvalidOperationException("A child with this name already exists under the same parent.");
                }if(samePatentAsset)
                {
                    throw new InvalidOperationException("Parent Cant be Its own Child");
                }
            }
            else
            {

                bool rootExists = await _db.Assets
                    .AnyAsync(a => a.ParentAssetId == null && a.Name == dto.Name);
                
                if (rootExists)
                {
                    throw new InvalidOperationException("A root with this name already exists.");
                }
            }
            

            var newAsset = new Asset
            {
                Name = dto.Name,
                ParentAssetId = parentId
            };

            _db.Assets.Add(newAsset);
           
            await _db.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"New asset added: {_currentUser.UserName ?? "System"}");

            return newAsset;
        }






        //public async Task DeleteNode(int id)
        //{
        //    var node = await _db.Assets.FirstOrDefaultAsync(a => a.Id == id);
        //    if (node == null)
        //        throw new Exception($"Id {id} does not exist");

        //    await RemoveChildrenAsync(node);
        //    _db.Assets.Remove(node);

        //    // Single SaveChanges call - no connection issues
        //    await _db.SaveChangesAsync();
        //}

        //private async Task RemoveChildrenAsync(Asset node)
        //{
        //    var children = await _db.Assets
        //        .Where(a => a.ParentAssetId == node.Id)
        //        .ToListAsync();

        //    foreach (var child in children)
        //    {
        //        await RemoveChildrenAsync(child);
        //        _db.Assets.Remove(child);
        //        // NO SaveChanges here - just mark for deletion
        //    }
        //}


        public async Task DeleteNode(int id)
        {
            using var db = _dbfactory.CreateDbContext();

            var node = await db.Assets.FirstOrDefaultAsync(a => a.Id == id);
            if (node == null)
                throw new Exception($"Id {id} does not exist");

            var allNodes = new List<Asset>();
            await CollectChildrenAsync(db, node, allNodes);
            allNodes.Add(node);

            foreach (var asset in allNodes)
            {
                db.Attach(asset);
                db.Remove(asset);
            }

            await db.SaveChangesAsync();
        }

        private async Task CollectChildrenAsync(AssetDbContext db, Asset node, List<Asset> result)
        {
            var children = await db.Assets
                .AsNoTracking()
                .Where(a => a.ParentAssetId == node.Id)
                .ToListAsync();

            foreach (var child in children)
            {
                result.Add(child);
                await CollectChildrenAsync(db, child, result);
            }
        }




        public async Task<int> DisplayCount()
        {
            // Get all assets from DB
            var allAssets = await _db.Assets.ToListAsync();

            if (allAssets == null || !allAssets.Any())
                return 0;

            // Find root nodes (ParentAssetId == null)
            var roots = allAssets.Where(a => a.ParentAssetId == null).ToList();

            int totalCount = 0;
            foreach (var root in roots)
            {
                totalCount += CountHelper(root, allAssets);
            }

            return totalCount;
        }

        // Recursive helper
        private int CountHelper(Asset node, List<Asset> allAssets)
        {
            int count = 1;

            // Find children of this node
            var children = allAssets.Where(a => a.ParentAssetId == node.Id).ToList();

            foreach (var child in children)
            {
                count += CountHelper(child, allAssets);
            }

            return count;
        }

        public async Task<int> MaxDepth()
        {
            var allAssets = await _db.Assets.ToListAsync();
            if (!allAssets.Any()) return 0;

            var roots = allAssets.Where(a => a.ParentAssetId == null).ToList();
            int maxDepth = 0;

            foreach (var root in roots)
            {
                int depth = MaxDepthHelper(root, allAssets, 1);
                if (depth > maxDepth) maxDepth = depth;
            }

            return maxDepth;
        }

        private int MaxDepthHelper(Asset node, List<Asset> allAssets, int currentDepth)
        {
            var children = allAssets.Where(a => a.ParentAssetId == node.Id).ToList();
            if (!children.Any()) return currentDepth;

            int maxChildDepth = currentDepth;
            foreach (var child in children)
            {
                int childDepth = MaxDepthHelper(child, allAssets, currentDepth + 1);
                if (childDepth > maxChildDepth) maxChildDepth = childDepth;
            }

            return maxChildDepth;
        }





        //public async Task ImportFromFile(List<FileAssetDto> fileAssets)
        //{
        //    // DEBUG: Print what we received
        //    Console.WriteLine("=== RECEIVED DATA ===");
        //    foreach (var asset in fileAssets)
        //    {
        //        Console.WriteLine($"TempId: {asset.TempId}, Name: {asset.Name}, TempParentId: {asset.TempParentId}");
        //    }
        //    Console.WriteLine("===================");

        //    var tempToDbIdMap = new Dictionary<int, int>();

        //    // Step 1: Insert roots first
        //    var roots = fileAssets.Where(a => a.TempParentId == null || a.TempParentId == 0).ToList();
        //    foreach (var root in roots)
        //    {
        //        var newAsset = new Asset { Name = root.Name, ParentAssetId = null };
        //        _db.Assets.Add(newAsset);
        //        await _db.SaveChangesAsync();
        //        tempToDbIdMap[root.TempId] = newAsset.Id; // Map TempId -> Auto-generated DbId
        //    }

        //    // Step 2: Insert children iteratively
        //    var remaining = fileAssets.Where(a => a.TempParentId != null && a.TempParentId != 0).ToList();
        //    while (remaining.Any())
        //    {
        //        var canInsert = remaining.Where(a => tempToDbIdMap.ContainsKey(a.TempParentId.Value)).ToList();
        //        if (!canInsert.Any()) break;

        //        foreach (var asset in canInsert)
        //        {
        //            var parentDbId = tempToDbIdMap[asset.TempParentId.Value]; // Get correct parent DbId
        //            Console.WriteLine($"Inserting: {asset.Name}, TempParentId: {asset.TempParentId}, MappedParentDbId: {parentDbId}");

        //            var newAsset = new Asset { Name = asset.Name, ParentAssetId = parentDbId };
        //            _db.Assets.Add(newAsset);
        //            await _db.SaveChangesAsync();
        //            tempToDbIdMap[asset.TempId] = newAsset.Id; // Map TempId -> Auto-generated DbId

        //            Console.WriteLine($"Created asset with DbId: {newAsset.Id}, ParentAssetId: {newAsset.ParentAssetId}");
        //        }

        //        // Remove processed assets from remaining list
        //        foreach (var processed in canInsert)
        //        {
        //            remaining.Remove(processed);
        //        }
        //    }
        //}

        public async Task ImportFromFile(List<FileAssetDto> fileAssets)
        {
            // DEBUG: Print received data
            Console.WriteLine("=== RECEIVED DATA ===");
            foreach (var asset in fileAssets)
            {
                Console.WriteLine($"TempId: {asset.TempId}, Name: {asset.Name}, TempParentId: {asset.TempParentId}");
            }
            Console.WriteLine("===================");

            var tempToDbIdMap = new Dictionary<int, int>();
            // Step 1: Check and insert root assets (TempParentId is null or 0)
            var roots = fileAssets.Where(a => a.TempParentId == null || a.TempParentId == 0).ToList();
            foreach (var root in roots)
            {
                // Check if an asset with the same Name and null ParentAssetId already exists
                var existingAsset = await _db.Assets
                    .FirstOrDefaultAsync(a => a.Name == root.Name && a.ParentAssetId == null);

                if (existingAsset != null)
                {
                    // Skip duplicate, map TempId to existing DbId
                    Console.WriteLine($"Skipping duplicate root: {root.Name}, Existing DbId: {existingAsset.Id}");
                    tempToDbIdMap[root.TempId] = existingAsset.Id;
                    continue;
                }

                var newAsset = new Asset { Name = root.Name, ParentAssetId = null };
                _db.Assets.Add(newAsset);
                await _db.SaveChangesAsync();
                tempToDbIdMap[root.TempId] = newAsset.Id;
                Console.WriteLine($"Created root asset: {newAsset.Name}, DbId: {newAsset.Id}");
            }

            // Step 2: Insert children iteratively
            var remaining = fileAssets.Where(a => a.TempParentId != null && a.TempParentId != 0).ToList();
            while (remaining.Any())
            {
                var canInsert = remaining.Where(a => tempToDbIdMap.ContainsKey(a.TempParentId.Value)).ToList();
                if (!canInsert.Any())
                {
                    Console.WriteLine("No more assets can be inserted (possible orphaned assets)");
                    break;
                }

                foreach (var asset in canInsert)
                {
                    var parentDbId = tempToDbIdMap[asset.TempParentId.Value];
                    // Check if an asset with the same Name and ParentAssetId already exists
                    var existingAsset = await _db.Assets
                        .FirstOrDefaultAsync(a => a.Name == asset.Name && a.ParentAssetId == parentDbId);

                    if (existingAsset != null)
                    {
                        // Skip duplicate, map TempId to existing DbId
                        Console.WriteLine($"Skipping duplicate child: {asset.Name}, ParentDbId: {parentDbId}, Existing DbId: {existingAsset.Id}");
                        tempToDbIdMap[asset.TempId] = existingAsset.Id;
                        continue;
                    }

                    var newAsset = new Asset { Name = asset.Name, ParentAssetId = parentDbId };
                    _db.Assets.Add(newAsset);
                    await _db.SaveChangesAsync();
                    tempToDbIdMap[asset.TempId] = newAsset.Id;
                    Console.WriteLine($"Created child asset: {newAsset.Name}, DbId: {newAsset.Id}, ParentAssetId: {newAsset.ParentAssetId}");
                }

                // Remove processed assets
                foreach (var processed in canInsert)
                {
                    remaining.Remove(processed);
                }
            }

            // Log any remaining assets that couldn't be inserted (orphans)
            if (remaining.Any())
            {
                Console.WriteLine("=== ORPHANED ASSETS ===");
                foreach (var orphan in remaining)
                {
                    Console.WriteLine($"TempId: {orphan.TempId}, Name: {orphan.Name}, TempParentId: {orphan.TempParentId} (Parent not found)");
                }
            }
        }


        public async Task<List<HitoryLog>> GetLogs()
        {
            try
            {
                var Logs =await  _db.HitoryLogs.ToListAsync();
                if (!Logs.Any())
                {
                    throw new Exception("No Logs Present");
                }
                var DisplayLogs = Logs.Select(x => new HitoryLog
                {
                    HistoryId = x.HistoryId,
                    TableName = x.TableName,
                    RecordId = x.RecordId,
                    Action = x.Action,
                    Description = x.Description,
                    ChangedBy = x.ChangedBy,
                    ChangedAt = x.ChangedAt
                }).ToList();

                Console.Write(DisplayLogs + "the logs from the service layer");

                return DisplayLogs;
            }
            catch (Exception ex)
            {
                 throw new Exception("Error fetching logs: " + ex.Message);
            }
        }



    }
}
