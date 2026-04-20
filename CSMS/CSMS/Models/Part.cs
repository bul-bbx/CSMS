using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class Part
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Manufacturer { get; set; }

        [MaxLength(50)]
        public string PartNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int StockQuantity { get; set; }

        public ICollection<RepairPart> RepairParts { get; set; }
    }
}
