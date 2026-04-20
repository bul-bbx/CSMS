using CSMS.Data;
using CSMS.Helpers;
using CSMS.Models;
using CSMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceTypesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditLogger _auditLogger;

        public ServiceTypesController(AppDbContext context, AuditLogger auditLogger)
        {
            _context = context;
            _auditLogger = auditLogger;
        }

        // GET: ServiceTypes
        public async Task<IActionResult> Index()
        {
            var serviceTypes = await _context.ServiceTypes.ToListAsync();
            return View(serviceTypes);
        }

        // GET: ServiceTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var serviceType = await _context.ServiceTypes
                .Include(st => st.Appointments)
                .Include(st => st.RepairHistories)
                .FirstOrDefaultAsync(st => st.Id == id);

            if (serviceType == null) return NotFound();

            return View(serviceType);
        }

        // GET: ServiceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ServiceTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceTypeViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var serviceType = new ServiceType
            {
                Name = vm.Name,
                Description = vm.Description,
                BasePrice = vm.BasePrice,
                EstimatedDurationMinutes = vm.EstimatedDurationMinutes
            };

            _context.ServiceTypes.Add(serviceType);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync(
                        "Service Type Created",
                        "ServiceType",
                        serviceType.Id
                    );

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null) return NotFound();

            var vm = new ServiceTypeViewModel
            {
                Id = serviceType.Id,
                Name = serviceType.Name,
                Description = serviceType.Description,
                BasePrice = serviceType.BasePrice,
                EstimatedDurationMinutes = serviceType.EstimatedDurationMinutes
            };

            return View(vm);
        }

        // POST: ServiceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceTypeViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid) return View(vm);

            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null) return NotFound();

            serviceType.Name = vm.Name;
            serviceType.Description = vm.Description;
            serviceType.BasePrice = vm.BasePrice;
            serviceType.EstimatedDurationMinutes = vm.EstimatedDurationMinutes;

            try
            {
                _context.Update(serviceType);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ServiceTypes.Any(st => st.Id == id))
                    return NotFound();
                else
                    throw;
            }

            await _auditLogger.LogAsync(
                        "Service Type Edited",
                        "ServiceType",
                        serviceType.Id
                    );

            return RedirectToAction(nameof(Index));
        }

        // GET: ServiceTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var serviceType = await _context.ServiceTypes
                .FirstOrDefaultAsync(st => st.Id == id);

            if (serviceType == null) return NotFound();

            return View(serviceType);
        }

        // POST: ServiceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null) return NotFound();

            _context.ServiceTypes.Remove(serviceType);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync(
                        "Service Type Deleted",
                        "ServiceType",
                        serviceType.Id
                    );

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceTypeExists(int id)
        {
            return _context.ServiceTypes.Any(st => st.Id == id);
        }
    }
}
