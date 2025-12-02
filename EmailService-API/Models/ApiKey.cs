using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmailService_API.Models
{
    [Table("apikeys")]
    public class ApiKey
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string KeyHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string KeyPrefix { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastUsedDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
