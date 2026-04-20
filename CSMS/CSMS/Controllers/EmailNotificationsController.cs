using CSMS.Data;
using CSMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin,Mechanic")]
[ApiController]
[Route("api/[controller]")]
public class EmailNotificationsController : Controller
{
    private readonly AppDbContext _context;

    public EmailNotificationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.EmailNotifications.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var notif = await _context.EmailNotifications.FindAsync(id);
        if (notif == null) return NotFound();
        return Ok(notif);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmailNotification notif)
    {
        _context.EmailNotifications.Add(notif);
        await _context.SaveChangesAsync();
        return Ok(notif);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmailNotification notif)
    {
        var existing = await _context.EmailNotifications.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Subject = notif.Subject;
        existing.Message = notif.Message;
        existing.SentAt = notif.SentAt;
        existing.Status = notif.Status;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var notif = await _context.EmailNotifications.FindAsync(id);
        if (notif == null) return NotFound();

        _context.EmailNotifications.Remove(notif);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
