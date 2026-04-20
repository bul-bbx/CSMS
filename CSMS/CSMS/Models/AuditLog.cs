using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        [Required, MaxLength(500)]
        public string Action { get; set; }

        [MaxLength(50)]
        public string EntityType { get; set; }

        public int? EntityId { get; set; }

        public DateTime Timestamp { get; set; }

        [MaxLength(50)]
        public string IpAddress { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}
