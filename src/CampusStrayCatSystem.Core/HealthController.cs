using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace CampusStrayCatSystem.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetHealth()
        {
            var connectionString = _configuration.GetConnectionString("Oracle");
            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, new
                {
                    database = "error",
                    message = "Connection string 'Oracle' not found in configuration."
                });
            }

            try
            {
                using var connection = new OracleConnection(connectionString);
                connection.Open();
                return Ok(new
                {
                    database = "connected",
                    message = "数据库连接正常。"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    database = "disconnected",
                    message = $"数据库连接失败：{ex.Message}"
                });
            }
        }
    }
}
