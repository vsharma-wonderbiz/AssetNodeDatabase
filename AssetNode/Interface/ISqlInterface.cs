using AssetNode.Models.Entities;
using AssetNode.Models.Dtos;
namespace AssetNode.Interface
{
    public interface ISqlInterface
    {
        public Task<List<AssetNodes>> GetJsonHierarchy();
        public Task<Asset> AddAsset(SqladdAsset dto);

        public Task DeleteNode(int id);

        public  Task<int> DisplayCount();

        public Task<int> MaxDepth();

        public Task ImportFromFile(List<FileAssetDto> fileAssets);
        public Task<List<HitoryLog>> GetLogs();
    }
}
