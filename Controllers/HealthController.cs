using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private static readonly DateTime StartTime = DateTime.UtcNow;

        // GET: api/health
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow
            });
        }

        /// GET: api/health/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        // GET: api/health/details
        [HttpGet("details")]
        public IActionResult GetDetails()
        {
            var process = Process.GetCurrentProcess();
            var memoryUsedMb = process.WorkingSet64 / (1024 * 1024);

            var details = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                uptime = (DateTime.UtcNow - StartTime).ToString(@"dd\.hh\:mm\:ss"),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                machineName = Environment.MachineName,
                memoryUsageMb = memoryUsedMb,
                dotNetVersion = Environment.Version.ToString()
            };

            return Ok(details);
        }
    }
}
