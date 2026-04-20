using CSMS.Data;
using CSMS.Helpers;
using CSMS.Models;
using CSMS.Models.ViewModels;
using CSMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CSMS.Controllers
{
    [Authorize(Roles = "Admin,Mechanic")]
    public class RepairPartsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditLogger _auditLogger;
        private readonly IEmailService _emailService;
        private readonly UserManager<IdentityUser> _userManager;

        public RepairPartsController(AppDbContext context, AuditLogger auditLogger, IEmailService emailService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _auditLogger = auditLogger;
            _emailService = emailService;
            _userManager = userManager;
        }

        // GET: RepairParts?repairOrderId=5
        public async Task<IActionResult> Index(int repairOrderId)
        {
            var repairOrder = await _context.RepairOrders
                .Include(ro => ro.RepairParts)
                    .ThenInclude(rp => rp.Part)
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.Car)
                .Include(ro => ro.Appointment)
                    .ThenInclude(a => a.ServiceType)
                .FirstOrDefaultAsync(ro => ro.Id == repairOrderId);

            if (repairOrder == null)
                return NotFound();

            return View(repairOrder);
        }

        // GET: RepairParts/Create?repairOrderId=5
        public async Task<IActionResult> Create(int repairOrderId)
        {
            var repairOrder = await _context.RepairOrders
                .Include(ro => ro.RepairParts)
                .FirstOrDefaultAsync(ro => ro.Id == repairOrderId);

            if (repairOrder == null)
                return NotFound();

            ViewBag.PartsList = new SelectList(await _context.Parts.ToListAsync(), "Id", "Name");

            var vm = new RepairPartViewModel
            {
                RepairOrderId = repairOrderId,
                Quantity = 1
            };

            return View(vm);
        }

        // POST: RepairParts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepairPartViewModel vm)
        {
            RepairOrder repairOrder = await _context.RepairOrders.FindAsync(vm.RepairOrderId);
            if (repairOrder.Status == "Completed")
            {
                return BadRequest("Cannot modify parts of a completed repair order.");
            }
            if (!ModelState.IsValid)
            {
                // Reload parts list if needed for dropdown
                ViewBag.PartsList = await _context.Parts
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                    .ToListAsync();

                return View(vm);
            }

            // Get the part from DB
            var part = await _context.Parts.FindAsync(vm.PartId);
            if (part == null)
            {
                ModelState.AddModelError("", "Selected part not found.");
                return View(vm);
            }

            // Check stock availability
            if (part.StockQuantity < vm.Quantity)
            {
                ModelState.AddModelError("", $"Not enough stock. Available: {part.StockQuantity}");
                return View(vm);
            }

            part.StockQuantity -= vm.Quantity;
            await _auditLogger.LogAsync(
                $"Used {vm.Quantity} of {part.Name}",
                "Part",
                part.Id
            );

            // If low stock
            if (part.StockQuantity <= 5)
            {
                await _auditLogger.LogAsync(
                    $"Low stock warning for {part.Name}: {part.StockQuantity} remaining",
                    "Part",
                    part.Id
                );
            }
            if (part.StockQuantity == 0)
            {
                await _auditLogger.LogAsync(
                    $"Part <b>{part.Name}</b> is OUT OF STOCK.",
                    "Part",
                    part.Id
                );
            }


            // Calculate total price
            var totalPrice = part.UnitPrice * vm.Quantity;

            // Create RepairPart
            var repairPart = new RepairPart
            {
                RepairOrderId = vm.RepairOrderId,
                PartId = vm.PartId,
                Quantity = vm.Quantity,
                TotalPrice = totalPrice
            };

            _context.RepairParts.Add(repairPart);
            await _context.SaveChangesAsync();

            if (part.StockQuantity <= 5)
            {
                await _auditLogger.LogAsync(
                    $"Low stock warning for {part.Name}: {part.StockQuantity} remaining",
                    "Part",
                    part.Id
                );
                await _emailService.SendAsync(
                    "kratos20050129@gmail.com",
                    "⚠ Low Stock Warning",
                    $@"
        <h2>Low Stock Alert</h2>
        <p>Part: <strong>{part.Name}</strong></p>
        <p>Remaining Stock: <strong>{part.StockQuantity}</strong></p>
        "
                );
            }
            if (part.StockQuantity == 0)
            {
                await _auditLogger.LogAsync(
                    $"Part <b>{part.Name}</b> is OUT OF STOCK.",
                    "Part",
                    part.Id
                );
                await _emailService.SendAsync(
                    "kratos20050129@gmail.com",
                    "❌ OUT OF STOCK",
                    $@"
        <h2>Out of Stock</h2>
        <p>Part: <strong>{part.Name}</strong></p>
        <p>Immediate restock required.</p>
        "
                );
            }

            return RedirectToAction("Index", new { repairOrderId = vm.RepairOrderId });
        }

        // GET: RepairParts/Edit/5
        // Example for Edit GET
        public async Task<IActionResult> Edit(int repairOrderId, int partId)
        {
            var repairPart = await _context.RepairParts
                .Include(rp => rp.Part)
                .Include(rp => rp.RepairOrder)
                .FirstOrDefaultAsync(rp => rp.RepairOrderId == repairOrderId && rp.PartId == partId);

            if (repairPart == null) return NotFound();

            var vm = new RepairPartViewModel
            {
                RepairOrderId = repairPart.RepairOrderId,
                PartId = repairPart.PartId,
                Quantity = repairPart.Quantity,
                TotalPrice = repairPart.TotalPrice
            };

            ViewBag.PartsList = new SelectList(await _context.Parts.ToListAsync(), "Id", "Name", repairPart.PartId);

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RepairPartViewModel vm)
        {
            RepairOrder repairOrder = await _context.RepairOrders.FindAsync(vm.RepairOrderId);
            if (repairOrder.Status == "Completed")
            {
                return BadRequest("Cannot modify parts of a completed repair order.");
            }
            if (!ModelState.IsValid)
            {
                // Reload parts list if needed for dropdown
                ViewBag.PartsList = await _context.Parts
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                    .ToListAsync();
                return View(vm);
            }

            // Find existing repair part
            var repairPart = await _context.RepairParts
                .Include(rp => rp.Part)
                .FirstOrDefaultAsync(rp => rp.Id == id);

            if (repairPart == null) return NotFound();

            // Check if part has changed
            if (repairPart.PartId != vm.PartId)
            {
                // Restore old part stock
                var oldPart = await _context.Parts.FindAsync(repairPart.PartId);
                oldPart.StockQuantity += repairPart.Quantity;

                // Reduce new part stock
                var newPart = await _context.Parts.FindAsync(vm.PartId);
                if (newPart.StockQuantity < vm.Quantity)
                {
                    ModelState.AddModelError("", $"Not enough stock for {newPart.Name}. Available: {newPart.StockQuantity}");
                    ViewBag.PartsList = await _context.Parts
                        .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                        .ToListAsync();
                    return View(vm);
                }

                newPart.StockQuantity -= vm.Quantity;
                await _auditLogger.LogAsync(
                    $"Used {vm.Quantity} of {newPart.Name}",
                    "Part",
                    newPart.Id
                );

                // If low stock
                if (newPart.StockQuantity <= 5)
                {
                    await _auditLogger.LogAsync(
                        $"Low stock warning for {newPart.Name}: {newPart.StockQuantity} remaining",
                        "Part",
                        newPart.Id
                    );
                    await _emailService.SendAsync(
                        "kratos20050129@gmail.com",
                        "⚠ Low Stock Warning",
                        $@"
        <h2>Low Stock Alert</h2>
        <p>Part: <strong>{newPart.Name}</strong></p>
        <p>Remaining Stock: <strong>{newPart.StockQuantity}</strong></p>
        "
                    );
                }
                if (newPart.StockQuantity == 0)
                {
                    await _auditLogger.LogAsync(
                        $"Part <b>{newPart.Name}</b> is OUT OF STOCK.",
                        "Part",
                        newPart.Id
                    );
                    await _emailService.SendAsync(
                        "kratos20050129@gmail.com",
                        "❌ OUT OF STOCK",
                        $@"
        <h2>Out of Stock</h2>
        <p>Part: <strong>{newPart.Name}</strong></p>
        <p>Immediate restock required.</p>
        "
                    );
                }

                repairPart.PartId = vm.PartId;
                repairPart.Part = newPart;


            }
            else
            {
                // Same part: adjust stock based on difference in quantity
                int diff = vm.Quantity - repairPart.Quantity; // positive if increasing
                if (repairPart.Part.StockQuantity < diff)
                {
                    ModelState.AddModelError("", $"Not enough stock for {repairPart.Part.Name}. Available: {repairPart.Part.StockQuantity}");
                    ViewBag.PartsList = await _context.Parts
                        .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                        .ToListAsync();
                    return View(vm);
                }
                repairPart.Part.StockQuantity -= diff;
            }

            // Update repair part properties
            repairPart.Quantity = vm.Quantity;
            repairPart.TotalPrice = repairPart.Part.UnitPrice * vm.Quantity;

            var oldQuantity = repairPart.Quantity;
            repairPart.Quantity = vm.Quantity;
            repairPart.TotalPrice = repairPart.Part.UnitPrice * vm.Quantity;

            await _auditLogger.LogAsync(
                $"Updated {repairPart.Part.Name} usage from {oldQuantity} to {vm.Quantity} for RepairOrder #{vm.RepairOrderId}",
                "RepairPart",
                repairPart.Id
            );

            // Save changes
            _context.Update(repairPart);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { repairOrderId = repairPart.RepairOrderId });
        }



        // GET: RepairParts/Delete
        public async Task<IActionResult> Delete(int repairOrderId, int partId)
        {
            var repairPart = await _context.RepairParts
                .Include(rp => rp.Part)
                .Include(rp => rp.RepairOrder)
                .FirstOrDefaultAsync(rp => rp.RepairOrderId == repairOrderId && rp.PartId == partId);

            if (repairPart == null) return NotFound();

            return View(repairPart);
        }

        // POST: RepairParts/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int repairOrderId, int partId)
        {
            RepairOrder repairOrder = await _context.RepairOrders.FindAsync(repairOrderId);
            if (repairOrder.Status == "Completed")
            {
                return BadRequest("Cannot modify parts of a completed repair order.");
            }
            var repairPart = await _context.RepairParts
                .FirstOrDefaultAsync(rp => rp.RepairOrderId == repairOrderId && rp.PartId == partId);

            if (repairPart == null) return NotFound();

            var part = await _context.Parts.FindAsync(repairPart.PartId);
            part.StockQuantity += repairPart.Quantity;

            await _auditLogger.LogAsync(
    $"Removed {repairPart.Quantity} of {part.Name} from RepairOrder #{repairPart.RepairOrderId}",
    "RepairPart",
    repairPart.Id
);

            _context.RepairParts.Remove(repairPart);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { repairOrderId = repairOrderId });
        }

    }
}
