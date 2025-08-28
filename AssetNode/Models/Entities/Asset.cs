using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AssetNode.Models.Entities
{
    public class Asset
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200,ErrorMessage ="Too Big Name")]
        public string Name { get; set; }


        public int? ParentAssetId { get; set; } // null means parent directly

        [ForeignKey("ParentAssetId")]
        [JsonIgnore]
        public Asset ParentAsset { get; set; }

        [JsonIgnore]
        public ICollection<Asset> Children { get; set; }


    }
}
