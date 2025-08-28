using AssetNode.Services.Sql;
using AssetNode.Data;
using AssetNode.Models.Entities;


namespace AssetNode.Data
{
    public class DbInitilaizer
    {
        public static void Initilaize(AssetDbContext context)
        {
            if (context.Assets.Any())
            {
                return;
            }

            // Step 1: Root insert
            var root = new Asset { Name = "Root Asset", ParentAssetId = null };
            context.Assets.Add(root);
            context.SaveChanges();

            // Step 2: Use generated Id of root for children
            var child1 = new Asset { Name = "Child 1", ParentAssetId = root.Id };
            var child2 = new Asset { Name = "Child 2", ParentAssetId = root.Id };

            context.Assets.AddRange(child1, child2);
            context.SaveChanges();
        }
    }
}
