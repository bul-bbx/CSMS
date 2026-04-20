using CSMS.Data;
using CSMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace CSMS.Helpers
{
    public class AuditLogger
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(AppDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityType = null, int? entityId = null)
        {
            var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
            var ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();

            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.Now,
                IpAddress = ip
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
