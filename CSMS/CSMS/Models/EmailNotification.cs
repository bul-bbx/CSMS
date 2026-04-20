using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class EmailNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public int? AppointmentId { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; } // Upcoming Repair, Invoice, Status Update

        [Required, MaxLength(100)]
        public string Subject { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        public DateTime ScheduledSendTime { get; set; }
        public DateTime? SentAt { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } // Pending, Sent, Failed

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }
    }
}
