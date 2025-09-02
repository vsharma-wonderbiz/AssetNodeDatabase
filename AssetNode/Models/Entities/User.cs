using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetNode.Models.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }   

        [Required]
        [MaxLength(50)]
        public string Username { get; set; } 

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }     

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User";  
    }
}
