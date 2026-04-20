using System.ComponentModel.DataAnnotations;

namespace CSMS.Models.ViewModels
{
    public class PartViewModel
    {
        public int Id { get; set; } // Needed for Edit

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Manufacturer { get; set; }

        [StringLength(50)]
        public string PartNumber { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}
