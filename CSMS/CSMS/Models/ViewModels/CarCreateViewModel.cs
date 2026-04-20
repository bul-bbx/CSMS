using System.ComponentModel.DataAnnotations;

namespace CSMS.Models.ViewModels
{
    public class CarCreateViewModel
    {
        [Required, MaxLength(50)]
        public string Brand { get; set; }

        [Required, MaxLength(50)]
        public string Model { get; set; }

        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required, MaxLength(17)]
        public string Vin { get; set; }

        [Required, MaxLength(15)]
        public string LicensePlate { get; set; }

        [MaxLength(50)]
        public string EngineType { get; set; }

        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }
    }

}
