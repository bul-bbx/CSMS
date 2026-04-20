using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public string MechanicId { get; set; } // FK to IdentityUser

        [Required]
        public int ServiceTypeId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } // Scheduled, InProgress, Completed, Cancelled

        [MaxLength(500)]
        public string Notes { get; set; }

        public bool ReminderSent { get; set; }

        // Navigation properties
        [ForeignKey("CarId")]
        public Car Car { get; set; }

        [ForeignKey("MechanicId")]
        public IdentityUser Mechanic { get; set; }

        [ForeignKey("ServiceTypeId")]
        public ServiceType ServiceType { get; set; }

        public RepairOrder RepairOrder { get; set; }
    }
}
