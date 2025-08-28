using AssetNode.Interface;
using AssetNode.Models.Dtos;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;


namespace AssetNode.Services
{
    public class AssetStroageService : IAssetStorage
    {
        private readonly string filePath = Path.Combine("Data", "JsonH.json");
        private readonly string filePath2 = Path.Combine("Data", "Import.txt");

        public List<AssetNodes> LoadHierarchy()
        {
            

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonSerializer.Deserialize<List<AssetNodes>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        public AssetNodes LoadHierarchysingle()
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonSerializer.Deserialize<AssetNodes>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }


        public void SaveHierarchy(List<AssetNodes> root)
        {
            string json = JsonSerializer.Serialize(root, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }

        


        public List<AssetNodes> ImportHierarchyFrom(List<AssetNodes> jsondata)
        {
            try
            {
                Console.WriteLine("Imported-Data: " + JsonSerializer.Serialize(jsondata));

                if (jsondata == null || jsondata.Count == 0)
                {
                    return null;
                }

                // No need to deserialize again. Just pass to hierarchy builder
                return BuildHierarchyFromFile(jsondata);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Some error occurred: {ex.Message}");
                return null;
            }
        }



        public List<AssetNodes> BuildHierarchyFromFile(List<AssetNodes> flatList)
        {
            Dictionary<int, AssetNodes> nodeLookup = new Dictionary<int, AssetNodes>();

            // First, add all nodes to the dictionary
            foreach (var node in flatList)
            {
                node.Children = new List<AssetNodes>(); // Ensure Children list is initialized
                nodeLookup[node.Id] = node;
            }

            List<AssetNodes> rootNodes = new List<AssetNodes>();

            // Build the tree by assigning children
            foreach (var node in flatList)
            {
                if (node.ParentAssetId.HasValue && nodeLookup.ContainsKey(node.ParentAssetId.Value))
                {
                    var parent = nodeLookup[node.ParentAssetId.Value];
                    parent.Children.Add(node);
                }
                else
                {
                    // If no parent found, it's a root node
                    rootNodes.Add(node);
                }
            }

            return rootNodes;
        }


        public void ReplaceNewData(List<AssetNodes> root)
        {
            var singleRoot = root.FirstOrDefault(); // Only take the first root node

            string json = JsonSerializer.Serialize(singleRoot, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePath, json);
        }



    }
}
