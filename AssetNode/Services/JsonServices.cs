using AssetNode.Models.Dtos;
using AssetNode.Interface;
using AssetNode.Data;
using AssetNode.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using AssetNode.Models.Entities;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;


namespace AssetNode.Services
{
    public class JsonServices : IJsonAssetInterface
    {
        private readonly string filePath = Path.Combine("Data", "JsonH.json");
        private readonly IAssetStorage _storageservice;

        public JsonServices(IAssetStorage storage)
        {
            _storageservice = storage;

        }

        public void AddAsset(List<AssetNodes> hierarchy, AssetDto dto)
        {
            if (IdExists(hierarchy, dto.Id))
            {
                throw new Exception($"Asset with ID {dto.Id} already exists!");
                // Ya tu API me BadRequest return kare
            }
            AssetNodes newNode = new AssetNodes
            {
                Id = dto.Id,
                Name = dto.Name,
                ParentAssetId = dto.ParentAssetId,
                Children = new List<AssetNodes>()
            };

            // Case 1: Add to root level
            if (dto.ParentAssetId == null || hierarchy.Count == 0)
            {
                hierarchy.Add(newNode);
                return;
            }
            else
            {
                AssetNodes parentnode = FindPArentid(hierarchy, dto.ParentAssetId.Value);
                if (parentnode != null)
                {
                    parentnode.Children.Add(newNode);
                }
                else
                {
                    Console.WriteLine("no parent found");
                }


            }

            // Case 2: Add as child node
            //File.WriteAllText("Data/JsonH.json", JsonSerializer.Serialize(hierarchy));
            _storageservice.SaveHierarchy(hierarchy);


        }

        private bool IdExists(List<AssetNodes> hierarchy, int id)
        {
            if (hierarchy == null || hierarchy.Count == 0)
            {
                return false;
            }
            foreach (var node in hierarchy)
            {
                if (node.Id == id)
                    return true;

                if (IdExists(node.Children, id))
                    return true;
            }
            return false;
        }


        public AssetNodes FindPArentid(List<AssetNodes> nodes, int id)
        {
            foreach (var node in nodes)
            {
                if (node.Id == id)
                {
                    return node;
                }
                var found = FindPArentid(node.Children, id);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        public List<AssetNodes> GetJsonHierarchy()
        {

            return _storageservice.LoadHierarchy() ?? new List<AssetNodes>(); // Wrap in list
        }

        public int DisplayCount()
        {
            var data = _storageservice.LoadHierarchy();
            if (data == null || !data.Any())
            {
                return 0;
            }
            int totalcount = 0;
            foreach (var item in data)
            {
                totalcount += CountHelper(item);
            }
            return totalcount;
        }

        public int CountHelper(AssetNodes root)
        {
            int count = 1;
            foreach (var child in root.Children)
            {
                count += CountHelper(child);
            }
            return count;
        }

        public void DeleteNode(List<AssetNodes> root, int id)
        {
            if (root == null || root.Count == 0)
            {
                throw new Exception("Data not found");
            }
            if (!IdExists(root, id))
            {
                throw new Exception($"Id {id} does not exist");
            }

            // Remove from root level
            for (int i = root.Count - 1; i >= 0; i--)
            {
                if (root[i].Id == id)
                {
                    root.RemoveAt(i);
                }
                else
                {
                    RemoveNodeRecursive(root[i], id);
                }
            }

            // Save updated data
            string filePath = "Data/JsonH.json";
            if (root.Count == 0)
            {
                File.WriteAllText(filePath, "");
            }
            else
            {
                File.WriteAllText(filePath, JsonSerializer.Serialize(root));
            }
        }

        private void RemoveNodeRecursive(AssetNodes currentNode, int id)
        {
            foreach (var child in currentNode.Children.ToList())
            {
                if (child.Id == id)
                {
                    currentNode.Children.Remove(child); // Remove from this node's children
                    return; // Done
                }
                else
                {
                    RemoveNodeRecursive(child, id); // Go deeper
                }
            }

        }

        public List<AssetNodes> ImportHeirarchyFromFile(List<AssetNodes> filedata)
        {
            var data = _storageservice.ImportHierarchyFrom(filedata);
            if (data == null)
            {
                return new List<AssetNodes>();
            }
            File.WriteAllText("Data/JsonH.json", JsonSerializer.Serialize(data));
            return data;
        }

        public int MaxDepth()
        {
            var data = _storageservice.LoadHierarchy();
            if (data == null || !data.Any())
            {
                return 0;
            }
            int[] depths = new int[data.Count];
            int MaxDepth = 0;
            foreach (var item in data)
            {
                if (item.ParentAssetId == null)
                {
                    int CurrentDepth = DepthHelper(item);
                    MaxDepth = Math.Max(CurrentDepth, MaxDepth);
                }
            }
            return MaxDepth;
        }

        public int DepthHelper(AssetNodes child)
        {
            if (child.Children.Count == 0 || child.Children == null)
            {
                return 1;
            }
            int maxdepth = 0;
            foreach (var item in child.Children)
            {
                int childdepth = DepthHelper(item);
                maxdepth = Math.Max(maxdepth, childdepth);
            }
            return 1 +
                maxdepth;
        }
        
    
     public List<AssetNodes> MergeHeirarchy(List<AssetNodes> existing, List<AssetNodes> newData)
        {
            foreach (var newnode in newData)
            {
                var exsistingnode = existing.FirstOrDefault(x => x.Id == newnode.Id);
                if (exsistingnode != null)
                {
                    exsistingnode.Children = MergeHeirarchy(exsistingnode.Children ?? new List<AssetNodes>(),
                                                     newnode.Children ?? new List<AssetNodes>());
                }
                else
                {
                    // Add new node
                    existing.Add(newnode);
                }
            }
            return existing;
        }
    }
}