using AssetNode.Models.Dtos;

namespace AssetNode.Interface
{
    public interface IAssetStorage
    {
        List<AssetNodes> LoadHierarchy();
        AssetNodes LoadHierarchysingle();
        void SaveHierarchy(List<AssetNodes> root);

        List<AssetNodes> ImportHierarchyFrom(List<AssetNodes> list);

        List<AssetNodes> BuildHierarchyFromFile(List<AssetNodes> list);

        void ReplaceNewData(List<AssetNodes> root);

    }
}
