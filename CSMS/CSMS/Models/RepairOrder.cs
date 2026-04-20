using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class RepairOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [MaxLength(500)]
        public string Diagnosis { get; set; }

        [MaxLength(1000)]
        public string WorkDescription { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } // Open, InProgress, Completed

        public DateTime? StartDatetime { get; set; }
        public DateTime? EndDatetime { get; set; }

        public decimal LaborHours { get; set; }
        public decimal LaborCost { get; set; }

        // Navigation
        [ForeignKey("AppointmentId")]
        public Appointment Appointment { get; set; }
        public RepairHistory RepairHistory { get; set; }
        public ICollection<RepairPart> RepairParts { get; set; }
        public Invoice Invoice { get; set; }

        [NotMapped]
        public decimal PartsTotal => RepairParts?.Sum(rp => rp.TotalPrice) ?? 0;

        [NotMapped]
        public int PartsCount => RepairParts?.Count ?? 0;

        [NotMapped]
        public decimal TotalCost => PartsTotal;
    }
}
