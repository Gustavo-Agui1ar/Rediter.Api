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

        /// <summary>
        /// Rota básica para verificar se a API está respondendo.
        /// GET: api/health
        /// </summary>
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Rota super leve apenas para testar a conectividade.
        /// GET: api/health/ping
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        /// <summary>
        /// Rota detalhada com informações do servidor e da aplicação.
        /// GET: api/health/details
        /// </summary>
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
