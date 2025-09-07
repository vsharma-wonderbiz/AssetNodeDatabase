using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetNode.Models.Entities
{
    public enum ActionType
    {
      Add,
      Update,
      Delete
    }
public class HitoryLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryId { get; set; }

        [Required]
        [MaxLength(255)]
        public string TableName { get; set; }

        [Required]
        public int RecordId { get; set; }

        [Required]
        [MaxLength(255)]
        public ActionType Action { get; set; } = ActionType.Add;

        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string ChangedBy { get; set; }   

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ChangedAt { get; set; }
    }
}
