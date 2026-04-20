using CSMS.Data;
using CSMS.Models;
using CSMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSMS.Controllers
{
    [Authorize]
    public class CarsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CarsController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cars
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var cars = await _context.Cars
                .Where(c => c.CustomerId == userId)
                .ToListAsync();
            return View(cars);
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .Include(c => c.Appointments)
                .FirstOrDefaultAsync(m => m.Id == id && m.CustomerId == _userManager.GetUserId(User));

            if (car == null) return NotFound();

            return View(car);
        }

        // GET: Cars/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarCreateViewModel ext)
        {
            if (!ModelState.IsValid) return View(ext);

            var car = new Car
            {
                Brand = ext.Brand,
                Model = ext.Model,
                Year = ext.Year,
                Vin = ext.Vin,
                LicensePlate = ext.LicensePlate,
                EngineType = ext.EngineType,
                Mileage = ext.Mileage,
                CustomerId = _userManager.GetUserId(User)
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == _userManager.GetUserId(User));

            if (car == null) return NotFound();

            var vm = new CarCreateViewModel
            {
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Vin = car.Vin,
                LicensePlate = car.LicensePlate,
                EngineType = car.EngineType,
                Mileage = car.Mileage
            };

            return View(vm);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CarCreateViewModel ext)
        {
            if (!ModelState.IsValid) return View(ext);

            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == _userManager.GetUserId(User));

            if (car == null) return NotFound();

            car.Brand = ext.Brand;
            car.Model = ext.Model;
            car.Year = ext.Year;
            car.Vin = ext.Vin;
            car.LicensePlate = ext.LicensePlate;
            car.EngineType = ext.EngineType;
            car.Mileage = ext.Mileage;

            try
            {
                _context.Update(car);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarExists(car.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == _userManager.GetUserId(User));

            if (car == null) return NotFound();

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.Id == id && c.CustomerId == _userManager.GetUserId(User));

            if (car == null) return NotFound();

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(c => c.Id == id);
        }
    }
}
