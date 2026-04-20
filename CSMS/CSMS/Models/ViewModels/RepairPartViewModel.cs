using CSMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSMS.Models.ViewModels
{
    public class RepairPartViewModel
    {
        public int Id { get; set; }

        [Required]
        public int RepairOrderId { get; set; }

        [Required]
        public int PartId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
