using CSMS.Models;
using CSMS.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CSMS.Services
{

    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }
}