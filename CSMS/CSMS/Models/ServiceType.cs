using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CSMS.Models
{
    public class ServiceType
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        public int EstimatedDurationMinutes { get; set; }

        // Ignore collections during binding
        [BindNever]
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        [BindNever]
        public ICollection<RepairHistory> RepairHistories { get; set; } = new List<RepairHistory>();
    }
}
