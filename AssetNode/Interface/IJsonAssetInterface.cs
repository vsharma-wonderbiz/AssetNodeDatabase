using AssetNode.Models.Dtos;

namespace AssetNode.Interface
{
    public interface IJsonAssetInterface
    {
        void AddAsset(List<AssetNodes>list,AssetDto dto);
         public List<AssetNodes> GetJsonHierarchy();
        public void DeleteNode(List<AssetNodes> data,int Id);

        public int DisplayCount();

        public int MaxDepth();

        List<AssetNodes> ImportHeirarchyFromFile(List<AssetNodes> data);

        List<AssetNodes> MergeHeirarchy(List<AssetNodes> existing, List<AssetNodes> newData);
    }
}
