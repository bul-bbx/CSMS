using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } // FK to IdentityUser

        [Required, MaxLength(50)]
        public string Brand { get; set; }

        [Required, MaxLength(50)]
        public string Model { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required, MaxLength(17)]
        public string Vin { get; set; } // unique constraint in DbContext

        [Required, MaxLength(15)]
        public string LicensePlate { get; set; }

        [MaxLength(50)]
        public string EngineType { get; set; }

        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        [BindNever]
        [ScaffoldColumn(false)]
        public IdentityUser Customer { get; set; }

        [BindNever]
        [ScaffoldColumn(false)]
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
