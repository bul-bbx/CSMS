using CSMS.Data;
using CSMS.Helpers;
using CSMS.Models;
using CSMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSMS.Controllers
{
    [Authorize(Roles = "Admin,Mechanic")]
    public class PartsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditLogger _auditLogger;

        public PartsController(AppDbContext context, AuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        // GET: Parts
        public async Task<IActionResult> Index()
        {
            var parts = await _context.Parts.ToListAsync();
            return View(parts);
        }

        // GET: Parts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Parts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var part = new Part
            {
                Name = vm.Name,
                Manufacturer = vm.Manufacturer,
                PartNumber = vm.PartNumber,
                UnitPrice = vm.UnitPrice,
                StockQuantity = vm.StockQuantity
            };

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Part Created", "Part", part.Id);

            return RedirectToAction(nameof(Index));
        }

        // GET: Parts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();

            var vm = new PartViewModel
            {
                Id = part.Id,
                Name = part.Name,
                Manufacturer = part.Manufacturer,
                PartNumber = part.PartNumber,
                UnitPrice = part.UnitPrice,
                StockQuantity = part.StockQuantity
            };

            return View(vm);
        }

        // POST: Parts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid) return View(vm);

            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();

            part.Name = vm.Name;
            part.Manufacturer = vm.Manufacturer;
            part.PartNumber = vm.PartNumber;
            part.UnitPrice = vm.UnitPrice;
            part.StockQuantity = vm.StockQuantity;

            _context.Update(part);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Part Edited", "Part", part.Id);

            return RedirectToAction(nameof(Index));
        }

        // GET: Parts/Delete/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (part == null) return NotFound();

            return View(part);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _context.Parts
                .FirstOrDefaultAsync(m => m.Id == id);

            if (part == null)
            {
                return NotFound();
            }

            return View(part); // Shows the confirmation page
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Part Deleted", "Part", part.Id);

            return RedirectToAction(nameof(Index));
        }
    }
}
