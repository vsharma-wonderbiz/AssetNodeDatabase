using AssetNode.Models.Entities;

namespace AssetNode.Models.Dtos
{
    public class SignalNodeDto
    {

        public int SignalId { get; set; }
        public string SignalName { get; set; }

        public string ValueType { get; set; }

        public string Description { get; set; }

        public int AssetID { get; set; }
    }
}
