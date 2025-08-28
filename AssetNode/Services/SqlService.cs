using AssetNode.Interface;
using AssetNode.Services.Sql;
using AssetNode.Models.Entities;
using Microsoft.EntityFrameworkCore;
using AssetNode.Models.Dtos;
using Microsoft.EntityFrameworkCore.Metadata;


namespace AssetNode.Services
{
    public class SqlService :ISqlInterface
    {
        private readonly AssetDbContext _db;

        public SqlService(AssetDbContext db)
        {
            _db = db;
        }
        
        public async Task<List<AssetNodes>> GetJsonHierarchy()
        {
            var allAssets = await _db.Assets.ToListAsync();

            var displaydata = allAssets.Select(x => new AssetNodes
            {
                Id = x.Id,
                Name = x.Name,
                ParentAssetId = x.ParentAssetId,
                Children = new List<AssetNodes>()
            }).ToList();

          
            var lookup = displaydata.ToLookup(x => x.ParentAssetId);

           
            foreach (var node in displaydata)
            {
                node.Children = lookup[node.Id].ToList();
            }
            var tree = lookup[null].ToList();

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
                if (childExists)
                {
                    throw new InvalidOperationException("A child with this name already exists under the same parent.");
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

            return newAsset;
        }






        public async Task DeleteNode(int id)
        {
            var node = _db.Assets.FirstOrDefault(a => a.Id == id);
            if (node == null)
                throw new Exception($"Id {id} does not exist");

            // Remove children recursively
            RemoveChildren(node);

            _db.Assets.Remove(node);
            _db.SaveChanges();
        }

        private void RemoveChildren(Asset node)
        {
            var children = _db.Assets.Where(a => a.ParentAssetId == node.Id).ToList();
            foreach (var child in children)
            {
                RemoveChildren(child);
                _db.Assets.Remove(child);
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
        //    using var transaction = _db.Database.BeginTransaction();
        //    try
        //    {
        //        // TempId -> DbId mapping
        //        var tempToDbId = new Dictionary<int, int>();

        //        // Step 1: Insert roots one by one to get their IDs immediately
        //        var roots = fileAssets
        //            .Where(a => a.TempParentId == null || a.TempParentId == 0)
        //            .ToList();

        //        foreach (var root in roots)
        //        {
        //            var newAsset = new Asset
        //            {
        //                Name = root.Name,
        //                ParentAssetId = null
        //            };

        //            _db.Assets.Add(newAsset);
        //            await _db.SaveChangesAsync(); // Save immediately to get the ID

        //            // Map temp ID to generated DB ID
        //            tempToDbId[root.TempId] = newAsset.Id;

        //            Console.WriteLine($"Inserted root: TempId={root.TempId} -> DbId={newAsset.Id}, Name={root.Name}");
        //        }

        //        // Step 2: Insert children level by level
        //        var remainingAssets = fileAssets
        //            .Where(a => a.TempParentId != null && a.TempParentId != 0)
        //            .ToList();

        //        int maxIterations = fileAssets.Count; // Prevent infinite loops
        //        int currentIteration = 0;

        //        while (remainingAssets.Any() && currentIteration < maxIterations)
        //        {
        //            currentIteration++;
        //            var assetsInsertedThisRound = new List<FileAssetDto>();

        //            // Find assets whose parents have been inserted
        //            foreach (var asset in remainingAssets.ToList())
        //            {
        //                if (asset.TempParentId.HasValue && tempToDbId.ContainsKey(asset.TempParentId.Value))
        //                {
        //                    var parentDbId = tempToDbId[asset.TempParentId.Value];

        //                    var newAsset = new Asset
        //                    {
        //                        Name = asset.Name,
        //                        ParentAssetId = parentDbId
        //                    };

        //                    _db.Assets.Add(newAsset);
        //                    await _db.SaveChangesAsync(); // Save immediately to get the ID

        //                    // Map temp ID to generated DB ID
        //                    tempToDbId[asset.TempId] = newAsset.Id;

        //                    Console.WriteLine($"Inserted child: TempId={asset.TempId} -> DbId={newAsset.Id}, Name={asset.Name}, ParentDbId={parentDbId}");

        //                    assetsInsertedThisRound.Add(asset);
        //                }
        //            }

        //            if (!assetsInsertedThisRound.Any())
        //            {
        //                // No progress made - there are orphaned assets
        //                var orphanedAssets = remainingAssets.Select(a => $"TempId={a.TempId}, Name={a.Name}, TempParentId={a.TempParentId}");
        //                var orphanedDetails = string.Join("; ", orphanedAssets);
        //                throw new InvalidOperationException($"Cannot insert assets: {orphanedDetails}. Their parents don't exist or weren't processed yet.");
        //            }

        //            // Remove successfully inserted assets from remaining list
        //            foreach (var inserted in assetsInsertedThisRound)
        //            {
        //                remainingAssets.Remove(inserted);
        //            }
        //        }

        //        if (remainingAssets.Any())
        //        {
        //            throw new InvalidOperationException($"Failed to insert {remainingAssets.Count} assets due to circular dependencies or missing parents.");
        //        }

        //        await transaction.CommitAsync();
        //        Console.WriteLine("Import completed successfully!");
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        Console.WriteLine($"Import failed: {ex.Message}");
        //        throw;
        //    }
        //}


        //public async Task ImportFromFile(List<FileAssetDto> fileAssets)
        //{
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
        //            var newAsset = new Asset { Name = asset.Name, ParentAssetId = parentDbId };
        //            _db.Assets.Add(newAsset);
        //            await _db.SaveChangesAsync();
        //            tempToDbIdMap[asset.TempId] = newAsset.Id; // Map TempId -> Auto-generated DbId
        //            remaining.Remove(asset);
        //        }
        //    }
        //}


        public async Task ImportFromFile(List<FileAssetDto> fileAssets)
        {
            // DEBUG: Print what we received
            Console.WriteLine("=== RECEIVED DATA ===");
            foreach (var asset in fileAssets)
            {
                Console.WriteLine($"TempId: {asset.TempId}, Name: {asset.Name}, TempParentId: {asset.TempParentId}");
            }
            Console.WriteLine("===================");

            var tempToDbIdMap = new Dictionary<int, int>();

            // Step 1: Insert roots first
            var roots = fileAssets.Where(a => a.TempParentId == null || a.TempParentId == 0).ToList();
            foreach (var root in roots)
            {
                var newAsset = new Asset { Name = root.Name, ParentAssetId = null };
                _db.Assets.Add(newAsset);
                await _db.SaveChangesAsync();
                tempToDbIdMap[root.TempId] = newAsset.Id; // Map TempId -> Auto-generated DbId
            }

            // Step 2: Insert children iteratively
            var remaining = fileAssets.Where(a => a.TempParentId != null && a.TempParentId != 0).ToList();
            while (remaining.Any())
            {
                var canInsert = remaining.Where(a => tempToDbIdMap.ContainsKey(a.TempParentId.Value)).ToList();
                if (!canInsert.Any()) break;

                foreach (var asset in canInsert)
                {
                    var parentDbId = tempToDbIdMap[asset.TempParentId.Value]; // Get correct parent DbId
                    Console.WriteLine($"Inserting: {asset.Name}, TempParentId: {asset.TempParentId}, MappedParentDbId: {parentDbId}");

                    var newAsset = new Asset { Name = asset.Name, ParentAssetId = parentDbId };
                    _db.Assets.Add(newAsset);
                    await _db.SaveChangesAsync();
                    tempToDbIdMap[asset.TempId] = newAsset.Id; // Map TempId -> Auto-generated DbId

                    Console.WriteLine($"Created asset with DbId: {newAsset.Id}, ParentAssetId: {newAsset.ParentAssetId}");
                }

                // Remove processed assets from remaining list
                foreach (var processed in canInsert)
                {
                    remaining.Remove(processed);
                }
            }
        }



    }
}
