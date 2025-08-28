namespace AssetNode.Models.Dtos
{
    public class AssetNodes
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentAssetId { get; set; }
        public List<AssetNodes> Children { get; set; } = new List<AssetNodes>();//to cretae the node to set the hirarchi
    }
}
