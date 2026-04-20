using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSMS.Models.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public int ServiceTypeId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public string Notes { get; set; }

        // Dropdown lists
        [BindNever]
        public IEnumerable<SelectListItem> Cars { get; set; } = new List<SelectListItem>();
        [BindNever]
        public IEnumerable<SelectListItem> ServiceTypes { get; set; } = new List<SelectListItem>();
    }

}
