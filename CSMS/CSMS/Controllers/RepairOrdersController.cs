using CSMS.Data;
using CSMS.Helpers;
using CSMS.Models;
using CSMS.Models.ViewModels;
using CSMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace CSMS.Controllers
{
    [Authorize(Roles = "Admin,Mechanic")]
    public class RepairOrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly AuditLogger _auditLogger;
        private readonly IEmailService _emailService;

        public RepairOrdersController(AppDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env, AuditLogger auditLogger, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
            _auditLogger = auditLogger;
            _emailService = emailService;
        }

        public async Task<IActionResult> IndexAll()
        {
            IQueryable<RepairOrder> query = _context.RepairOrders
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.ServiceType)
                .Include(ro => ro.RepairParts);

            var repairOrders = await query.ToListAsync();

            // Count total parts quantity per repair order
            var repairOrdersWithPartsCount = repairOrders.Select(ro => new
            {
                RepairOrder = ro,
                PartsCount = ro.RepairParts?.Sum(rp => rp.Quantity) ?? 0
            }).ToList();

            ViewBag.RepairOrdersWithPartsCount = repairOrdersWithPartsCount;

            return View(repairOrders);
        }

        // GET: RepairOrders
        public async Task<IActionResult> Index(int? appointmentId)
        {
            IQueryable<RepairOrder> query = _context.RepairOrders
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.ServiceType)
                .Include(ro => ro.RepairParts);

            if (appointmentId.HasValue)
            {
                query = query.Where(ro => ro.AppointmentId == appointmentId.Value);
                ViewBag.AppointmentId = appointmentId.Value;
            }

            var repairOrders = await query.ToListAsync();

            // Count total parts quantity per repair order
            var repairOrdersWithPartsCount = repairOrders.Select(ro => new
            {
                RepairOrder = ro,
                PartsCount = ro.RepairParts?.Sum(rp => rp.Quantity) ?? 0
            }).ToList();

            ViewBag.RepairOrdersWithPartsCount = repairOrdersWithPartsCount;
            ViewBag.AppointmentId = appointmentId;

            return View(repairOrders);
        }

        // GET: RepairOrders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.RepairOrders
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                .Include(ro => ro.Appointment.ServiceType)
                .Include(ro => ro.RepairParts)
                    .ThenInclude(rp => rp.Part)
                .Include(ro => ro.Invoice)
                .FirstOrDefaultAsync(ro => ro.Id == id);

            if (order == null) return NotFound();

            ViewBag.repairPartsCount = order.RepairParts?.Sum(rp => rp.Quantity) ?? 0;

            return View(order);
        }

        // GET: RepairOrders/Create?appointmentId=5
        [HttpGet]
        public async Task<IActionResult> Create(int appointmentId)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments
                .Include(a => a.Car)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return NotFound();

            ViewBag.Appointment = appointment;

            var model = new RepairOrderViewModel
            {
                AppointmentId = appointmentId,
                Status = "Open",
                StartDatetime = DateTime.Now
            };

            return View("Create", model);
        }

        // POST: RepairOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepairOrderViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments
                .Include(a => a.Car)
                .FirstOrDefaultAsync(a => a.Id == vm.AppointmentId);

            if (appointment == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Appointment = appointment;
                return View(vm);
            }

            var repairOrder = new RepairOrder
            {
                AppointmentId = vm.AppointmentId,
                Diagnosis = vm.Diagnosis,
                WorkDescription = vm.WorkDescription,
                Status = vm.Status,
                StartDatetime = vm.StartDatetime,
                EndDatetime = vm.EndDatetime
            };

            _context.RepairOrders.Add(repairOrder);
            appointment.RepairOrder = repairOrder;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Appointments", new { id = vm.AppointmentId });
        }

        // GET: RepairOrders/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var repairOrder = await _context.RepairOrders
     .Include(ro => ro.Appointment)
         .ThenInclude(a => a.Car)
             .ThenInclude(c => c.Customer)
     .Include(ro => ro.RepairParts)
         .ThenInclude(rp => rp.Part)
     .FirstOrDefaultAsync(ro => ro.Id == id);
            if (repairOrder == null) return NotFound();

            var vm = new RepairOrderViewModel
            {
                Id = repairOrder.Id,
                AppointmentId = repairOrder.AppointmentId,
                Diagnosis = repairOrder.Diagnosis,
                WorkDescription = repairOrder.WorkDescription,
                Status = repairOrder.Status,
                StartDatetime = repairOrder.StartDatetime,
                EndDatetime = repairOrder.EndDatetime
            };

            ViewData["Appointments"] = await _context.Appointments
                .Include(a => a.Car)
                .Include(a => a.ServiceType)
                .Select(a => new { a.Id, Display = a.Car.Brand + " " + a.Car.Model + " (" + a.AppointmentDate + ")" })
                .ToListAsync();

            return View(vm);
        }

        // POST: RepairOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RepairOrderViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var repairOrder = await _context.RepairOrders.FindAsync(id);
            if (repairOrder == null) return NotFound();

            if (repairOrder.Status == "Completed")
            {
                return BadRequest("Cannot modify parts of a completed repair order.");
            }

            repairOrder.Diagnosis = vm.Diagnosis;
            repairOrder.WorkDescription = vm.WorkDescription;
            repairOrder.Status = vm.Status;
            repairOrder.StartDatetime = vm.StartDatetime;
            repairOrder.EndDatetime = vm.EndDatetime;

            _context.Update(repairOrder);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: RepairOrders/Start/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var order = await _context.RepairOrders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Open")
                return BadRequest();

            order.Status = "InProgress";
            order.StartDatetime = DateTime.Now;

            await _auditLogger.LogAsync($"Started RepairOrder #{id}", "RepairOrder", id);

            await _context.SaveChangesAsync();

            var appointment = await _context.Appointments.FindAsync(order.AppointmentId);
            var car = await _context.Cars.FindAsync(appointment.CarId);
            var user = await _userManager.FindByIdAsync(car.CustomerId);

            await _emailService.SendAsync(
                user.Email,
                "Your repair has started",
                $@"
        <h2>Repair Started</h2>
        <p>Your repair order #{order.Id} has been Started.</p>
        "
            );

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: RepairOrders/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            // 1️⃣ Load repair order with related data
            var order = await _context.RepairOrders
    .Include(ro => ro.Appointment)
        .ThenInclude(a => a.Car)
            .ThenInclude(c => c.Customer) // 🔥 THIS LINE
    .Include(ro => ro.RepairParts)
        .ThenInclude(rp => rp.Part)
    .FirstOrDefaultAsync(ro => ro.Id == id);


            if (order == null) return NotFound();

            // 2️⃣ Prevent double invoicing
            var existingInvoice = await _context.Invoices
                .AnyAsync(i => i.RepairOrderId == id);

            if (existingInvoice)
                return BadRequest("Invoice already exists.");

            // 3️⃣ Complete repair order
            order.Status = "Completed";
            order.EndDatetime = DateTime.Now;

            // Log completion
            await _auditLogger.LogAsync($"Completed RepairOrder #{id}", "RepairOrder", id);

            // 4️⃣ Calculate costs
            var partsSubtotal = order.RepairParts?.Sum(rp => rp.TotalPrice) ?? 0m;
            var laborSubtotal = order.LaborCost;
            var subtotal = partsSubtotal + laborSubtotal;
            var tax = subtotal * 0.20m; // 20% VAT
            var total = subtotal + tax;

            // 5️⃣ Create Invoice
            var invoice = new Invoice
            {
                RepairOrderId = order.Id,
                CustomerId = order.Appointment.Car.CustomerId,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMddHHmmss}",
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                Subtotal = subtotal,
                Tax = tax,
                TotalAmount = total,
                Status = "Issued",
                Items = new List<InvoiceItem>()
            };

            // 6️⃣ Invoice Items – Labor
            if (laborSubtotal > 0)
            {
                invoice.Items.Add(new InvoiceItem
                {
                    Description = "Labor",
                    Quantity = 1,
                    UnitPrice = laborSubtotal,
                    TotalPrice = laborSubtotal
                });
            }

            // 7️⃣ Invoice Items – Parts
            foreach (var rp in order.RepairParts)
            {
                invoice.Items.Add(new InvoiceItem
                {
                    Description = rp.Part.Name,
                    Quantity = rp.Quantity,
                    UnitPrice = rp.Part.UnitPrice,
                    TotalPrice = rp.TotalPrice
                });
            }

            var pdfFileName = $"{invoice.InvoiceNumber}.pdf";
            var pdfFolder = Path.Combine(_env.WebRootPath, "invoices");
            Directory.CreateDirectory(pdfFolder);
            var pdfFilePath = Path.Combine(pdfFolder, pdfFileName);

            InvoicePdfGenerator.Generate(invoice, pdfFilePath);

            // 8️⃣ Update invoice PdfPath
            invoice.PdfPath = $"/invoices/{pdfFileName}";
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            var appointment = await _context.Appointments.FindAsync(order.AppointmentId);
            var car = await _context.Cars.FindAsync(appointment.CarId);
            var user = await _userManager.FindByIdAsync(car.CustomerId);

            await _emailService.SendAsync(
                user.Email,
                "Your repair is complete",
                $@"
        <h2>Repair Completed</h2>
        <p>Your repair order #{order.Id} has been completed.</p>
        <p>Total: <strong>{invoice.TotalAmount:C}</strong></p>
        <p>You can download your invoice from the portal.</p>
        "
            );

            return RedirectToAction(nameof(Details), new { id });
        }



        // POST: RepairOrders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.RepairOrders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = "Cancelled";
            order.EndDatetime = DateTime.Now;

            await _auditLogger.LogAsync($"Cancelled RepairOrder #{id}", "RepairOrder", id);

            await _context.SaveChangesAsync();


            var appointment = await _context.Appointments.FindAsync(order.AppointmentId);
            var car = await _context.Cars.FindAsync(appointment.CarId);
            var user = await _userManager.FindByIdAsync(car.CustomerId);

            await _emailService.SendAsync(
                user.Email,
                "Your repair is cancelled",
                $@"
        <h2>Repair Cancelled</h2>
        <p>Your repair order #{order.Id} has been cancelled.</p>
        "
            );

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: RepairOrders/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var repairOrder = await _context.RepairOrders
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.ServiceType)
                .FirstOrDefaultAsync(ro => ro.Id == id);

            if (repairOrder == null) return NotFound();

            return View(repairOrder);
        }

        // POST: RepairOrders/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var repairOrder = await _context.RepairOrders
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.ServiceType)
                .FirstOrDefaultAsync(ro => ro.Id == id);

            if (repairOrder == null) return NotFound();

            _context.RepairOrders.Remove(repairOrder);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync($"Deleted RepairOrder #{id}", "RepairOrder", id);

            return RedirectToAction(nameof(Index));
        }

        // GET: RepairOrders/History?carId=5
        public async Task<IActionResult> History(int? carId)
        {
            var query = _context.RepairOrders
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                        .ThenInclude(c => c.Customer)
                .Include(ro => ro.RepairParts)
                    .ThenInclude(rp => rp.Part)
                .Where(ro => ro.Status == "Completed")
                .AsQueryable();

            if (carId.HasValue)
            {
                query = query.Where(ro => ro.Appointment.CarId == carId.Value);
                ViewBag.CarId = carId.Value;
            }

            var completedOrders = await query
                .OrderByDescending(ro => ro.EndDatetime)
                .ToListAsync();

            var repairTimes = completedOrders.ToDictionary(
        ro => ro.Id,
        ro => new {
            Start = ro.StartDatetime != null ? ro.StartDatetime.ToString() : "N/A",
            End = ro.EndDatetime.HasValue ? ro.EndDatetime.Value.ToString() : "N/A"
        }
    );

            ViewBag.RepairTimes = repairTimes;

            return View(completedOrders);

        }

    }
}
