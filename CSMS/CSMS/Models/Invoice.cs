using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSMS.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RepairOrderId { get; set; }

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; } // unique in DbContext

        [Required]
        public string CustomerId { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        public DateTime? DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } // Draft, Issued, Paid, Cancelled

        [MaxLength(200)]
        public string PdfPath { get; set; }

        // Navigation
        [ForeignKey("RepairOrderId")]
        public RepairOrder RepairOrder { get; set; }

        [ForeignKey("CustomerId")]
        public IdentityUser Customer { get; set; }

        public ICollection<InvoiceItem> Items { get; set; }
    }
}
