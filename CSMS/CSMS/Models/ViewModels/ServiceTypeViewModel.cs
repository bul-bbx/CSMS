using System.ComponentModel.DataAnnotations;

namespace CSMS.Models.ViewModels
{
    public class ServiceTypeViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        [Range(0, 1440)]
        public int EstimatedDurationMinutes { get; set; }
    }
}
