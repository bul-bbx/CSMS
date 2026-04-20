using System.ComponentModel.DataAnnotations;

namespace CSMS.Models.ViewModels
{
    public class RepairOrderViewModel
    {
        [Key]
        public int Id { get; set; }
        public int AppointmentId { get; set; }

        [MaxLength(500)]
        public string Diagnosis { get; set; }

        [MaxLength(1000)]
        public string WorkDescription { get; set; }

        [Required]
        public string Status { get; set; } = "Open";

        public DateTime? StartDatetime { get; set; }
        public DateTime? EndDatetime { get; set; }
    }
}
