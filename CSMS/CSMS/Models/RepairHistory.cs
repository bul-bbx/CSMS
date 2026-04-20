using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class RepairHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RepairOrderId { get; set; }

        [Required]
        public int ServiceTypeId { get; set; }

        [Required]
        public DateTime CompletedDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        [ForeignKey("RepairOrderId")]
        public RepairOrder RepairOrder { get; set; }

        [ForeignKey("ServiceTypeId")]
        public ServiceType ServiceType { get; set; }
    }
}
