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
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly AuditLogger _auditLogger;

        public AppointmentsController(AppDbContext context, UserManager<IdentityUser> userManager, IEmailService emailService, AuditLogger auditLogger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _auditLogger = auditLogger;
        }

        // GET: Appointments
        [HttpGet]
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var selectedDate = new DateTime(
                year ?? DateTime.Today.Year,
                month ?? DateTime.Today.Month,
                1
            );

            var userId = _userManager.GetUserId(User);

            var appointments = await _context.Appointments
                .Include(a => a.Car)
                .Include(a => a.ServiceType)
                .Where(a =>
                    a.AppointmentDate.Year == selectedDate.Year &&
                    a.AppointmentDate.Month == selectedDate.Month &&
                    a.Status != "Completed" &&
                    a.Status != "Cancelled"
                )
                .ToListAsync();

            
            ViewBag.Year = selectedDate.Year;
            ViewBag.Month = selectedDate.Month;

            return View(appointments);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Car)
                .Include(a => a.ServiceType)
                .Include(a => a.Mechanic)
                .FirstOrDefaultAsync(a =>
                    a.Id == id);

            if (appointment == null)
                return NotFound();

            ViewBag.RO = await _context.RepairOrders
    .Include(ro => ro.Appointment)   // optional
    .FirstOrDefaultAsync(ro => ro.AppointmentId == appointment.Id);

            return View(appointment);
        }

        private async Task PopulateDropdowns(AppointmentViewModel vm)
        {
            var userId = _userManager.GetUserId(User);

            vm.Cars = await _context.Cars
                .Where(c => c.CustomerId == userId)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Brand} {c.Model} ({c.LicensePlate})"
                })
                .ToListAsync();

            vm.ServiceTypes = await _context.ServiceTypes
                .Select(st => new SelectListItem
                {
                    Value = st.Id.ToString(),
                    Text = st.Name
                })
                .ToListAsync();
        }

        // GET: Appointments/Create
        public async Task<IActionResult> Create(string date)
        {
            var vm = new AppointmentViewModel();

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsedDate))
                vm.AppointmentDate = parsedDate;

            await PopulateDropdowns(vm);
            return View(vm);
        }



        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel vm)
        {

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            var appointment = new Appointment
            {
                CarId = vm.CarId,
                ServiceTypeId = vm.ServiceTypeId,
                AppointmentDate = vm.AppointmentDate,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                Notes = vm.Notes,
                Status = "Scheduled",
                MechanicId = _context.Users.First().Id
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var car = await _context.Cars.FindAsync(appointment.CarId);
            var user = await _userManager.FindByIdAsync(car.CustomerId);

            await _auditLogger.LogAsync("Appointment Created", "Appointment", appointment.Id);
            await _emailService.SendAsync(
                user.Email,
                "Appointment Scheduled",
                $"Your appointment for {appointment.AppointmentDate:d} has been scheduled."
            );

            return RedirectToAction(nameof(Index));
        }



        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Car)
                .Include(a => a.ServiceType)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var vm = new AppointmentViewModel
            {
                Id = appointment.Id,
                CarId = appointment.CarId,
                ServiceTypeId = appointment.ServiceTypeId,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Notes = appointment.Notes,
                Cars = _context.Cars.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Brand + " " + c.Model+ " ("+c.LicensePlate+")" }).ToList(),
                ServiceTypes = _context.ServiceTypes.Select(st => new SelectListItem { Value = st.Id.ToString(), Text = st.Name }).ToList()
            };

            return View(vm);
        }


        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppointmentViewModel ext)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(ext);
            }

            var appointment = await _context.Appointments
                .Include(a => a.Car)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.Car.CustomerId == _userManager.GetUserId(User));

            if (appointment == null)
                return NotFound();

            appointment.CarId = ext.CarId;
            appointment.ServiceTypeId = ext.ServiceTypeId;
            appointment.AppointmentDate = ext.AppointmentDate;
            appointment.StartTime = ext.StartTime;
            appointment.EndTime = ext.EndTime;
            appointment.Notes = ext.Notes;

            _context.Update(appointment);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Appointment Edited", "Appointment", appointment.Id);

            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Car)
                .Include(a => a.ServiceType)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.Car.CustomerId == _userManager.GetUserId(User));

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            await _auditLogger.LogAsync("Appointment Deleted", "Appointment", appointment.Id);

            return RedirectToAction(nameof(Index));
        }

        // Shared dropdown loader
        private async Task LoadDropdowns()
        {
            var userId = _userManager.GetUserId(User);

            var cars = await _context.Cars
                .Where(c => c.CustomerId == userId)
                .ToListAsync();
            ViewData["Cars"] = new SelectList(cars, "Id", "LicensePlate");

            var services = await _context.ServiceTypes.ToListAsync();
            ViewData["ServiceTypes"] = new SelectList(services, "Id", "Name");
        }

        public async Task<IActionResult> DayDetails(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            var userId = _userManager.GetUserId(User);

            var appointments = await _context.Appointments
    .Include(a => a.Car)
    .Include(a => a.ServiceType)
    .Include(a => a.Mechanic) // ✅ THIS WAS MISSING
    .Where(a =>
        a.AppointmentDate.Date == date.Date)
    .ToListAsync();

            ViewBag.Date = date;
            ViewBag.ActiveCount = appointments.Count(a =>
                a.Status != "Completed" && a.Status != "Cancelled"
            );

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            if (appointment.Status == "Scheduled")
            {
                appointment.Status = "InProgress";
                await _context.SaveChangesAsync();
            }

            var car = await _context.Cars.FindAsync(appointment.CarId);
            var user = await _userManager.FindByIdAsync(car.CustomerId);

            await _auditLogger.LogAsync("Appointment Started", "Appointment", appointment.Id);
            await _emailService.SendAsync(
                user.Email,
                "Appointment Started",
                $"Your appointment for {appointment.AppointmentDate:d} has been Started."
            );

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            if (appointment.Status == "InProgress")
            {
                appointment.Status = "Completed";
                await _context.SaveChangesAsync();
            }

            var car = await _context.Cars.FindAsync(appointment.CarId);
            var user = await _userManager.FindByIdAsync(car.CustomerId);
            await _auditLogger.LogAsync("Appointment Completed", "Appointment", appointment.Id);
            await _emailService.SendAsync(
                user.Email,
                "Appointment Completed",
                $"Your appointment for {appointment.AppointmentDate:d} has been Completed."
            );

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            if (appointment.Status == "Scheduled")
            {
                appointment.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }

            await _auditLogger.LogAsync("Appointment Cancelled", "Appointment", appointment.Id);

            var car = await _context.Cars.FindAsync(appointment.CarId);
            var user = await _userManager.FindByIdAsync(car.CustomerId);

            await _emailService.SendAsync(
                user.Email,
                "Appointment Cancelled",
                $"Your appointment for {appointment.AppointmentDate:d} has been Cancelled."
            );

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
