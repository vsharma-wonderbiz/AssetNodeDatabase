using Newtonsoft.Json;

namespace AssetNode.Models.Dtos
{
    public class FileAssetDto
    {
        [JsonProperty("Id")]
        public int TempId { get; set; }

        [JsonProperty("ParentId")]
        public int? TempParentId { get; set; } // Temporary Parent ID from file
        public string Name { get; set; }
    }
}
