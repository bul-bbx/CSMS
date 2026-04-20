using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class RepairPart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RepairOrderId { get; set; }

        [Required]
        public int PartId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        // Navigation
        [ForeignKey("RepairOrderId")]
        public RepairOrder RepairOrder { get; set; }

        [ForeignKey("PartId")]
        public Part Part { get; set; }
    }
}
