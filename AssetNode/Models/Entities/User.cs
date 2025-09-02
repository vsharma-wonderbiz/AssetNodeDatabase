using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetNode.Models.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }   // Auto-generated ID

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }  // Unique index will be in DbContext

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }     // Unique index will be in DbContext

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";  // Default value
    }
}
