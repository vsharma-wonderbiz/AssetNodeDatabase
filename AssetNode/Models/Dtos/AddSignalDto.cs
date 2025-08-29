namespace AssetNode.Models.Dtos
{
    public class AddSignalDto
    {
        public string SignalName { get; set; }
        public string ValueType { get; set; }

        public string Description { get; set; }

        public int AssetId { get; set; }
    }
}
