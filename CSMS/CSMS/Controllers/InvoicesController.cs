using CSMS.Data;
using CSMS.Helpers;
using CSMS.Models;
using CSMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CSMS.Controllers
{
    
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly AuditLogger _auditLogger;

        public InvoicesController(AppDbContext context, UserManager<IdentityUser> userManager, IEmailService emailService, AuditLogger auditLogger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _auditLogger = auditLogger;
        }

        // ============================
        // INDEX
        // ============================
        [Authorize(Roles = "Admin,Mechanic")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var invoices = await _context.Invoices
    .Include(i => i.Customer)  // <-- this is the fix
    .Include(i => i.RepairOrder)
        .ThenInclude(ro => ro.Appointment)
            .ThenInclude(a => a.Car)
    .ToListAsync();

            return View(invoices);
        }

        // ============================
        // DETAILS
        // ============================
        public async Task<IActionResult> Details(bool isMy, int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)      // <-- include customer
                .Include(i => i.RepairOrder)   // optional if you need repair order info
                .Include(i => i.Items)         // include items for the invoice
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null) return NotFound();

            ViewBag.isMy = isMy;

            return View(invoice);
        }

        // ============================
        // CREATE (GET)
        // ============================
        [Authorize(Roles = "Admin,Mechanic")]
        public async Task<IActionResult> Create(int repairOrderId)
        {
            var userId = _userManager.GetUserId(User);

            var repairOrder = await _context.RepairOrders
                .Include(ro => ro.RepairParts)
                    .ThenInclude(rp => rp.Part)
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                .FirstOrDefaultAsync(ro =>
                    ro.Id == repairOrderId &&
                    ro.Status == "Completed");

            if (repairOrder == null)
                return BadRequest("Invoice can only be created for completed repair orders.");

            // Prevent duplicate invoices
            bool invoiceExists = await _context.Invoices
                .AnyAsync(i => i.RepairOrderId == repairOrderId);

            if (invoiceExists)
                return BadRequest("An invoice already exists for this repair order.");

            return View(repairOrder);
        }

        // ============================
        // CREATE (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanic")]
        public async Task<IActionResult> CreateConfirmed(int repairOrderId)
        {
            var userId = _userManager.GetUserId(User);

            var repairOrder = await _context.RepairOrders
                .Include(ro => ro.RepairParts)
                    .ThenInclude(rp => rp.Part)
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                .FirstOrDefaultAsync(ro =>
                    ro.Id == repairOrderId &&
                    ro.Status == "Completed");

            if (repairOrder == null)
                return BadRequest("Invalid repair order.");

            // Calculate totals
            decimal subtotal = repairOrder.RepairParts.Sum(rp => rp.TotalPrice);
            decimal tax = subtotal * 0.15m; // 15% example
            decimal total = subtotal + tax;

            var invoice = new Invoice
            {
                RepairOrderId = repairOrder.Id,
                CustomerId = userId,
                InvoiceNumber = GenerateInvoiceNumber(),
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Subtotal = subtotal,
                Tax = tax,
                TotalAmount = total,
                Status = "Issued"
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            var customer = await _userManager.FindByIdAsync(userId);

            await _emailService.SendAsync(
    customer.Email,
    "Invoice Issued",
    $@"
    <h2>Invoice {invoice.InvoiceNumber}</h2>
    <p>Total: {invoice.TotalAmount:C}</p>
    <a href='{invoice.PdfPath}'>Download PDF</a>
    "
);

            await _auditLogger.LogAsync("Invoice Created", "Invoice", invoice.Id);

            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }

        // ============================
        // MARK AS PAID
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanic")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound();

            invoice.Status = "Paid";
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Invoice Paid", "Invoice", invoice.Id);

            return RedirectToAction(nameof(Details), new { id });
        }

        // ============================
        // CANCEL
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Mechanic")]
        public async Task<IActionResult> Cancel(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound();

            if (invoice.Status == "Paid")
                return BadRequest("Paid invoices cannot be cancelled.");

            invoice.Status = "Cancelled";
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Invoice Cancelled", "Invoice", invoice.Id);

            return RedirectToAction(nameof(Details), new { id });
        }

        // ============================
        // HELPERS
        // ============================
        [Authorize(Roles = "Admin,Mechanic")]
        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMddHHmmss}";
        }

        public async Task<IActionResult> My()
        {
            var userId = _userManager.GetUserId(User);

            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.CustomerId == userId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(invoices);
        }
    }
}
