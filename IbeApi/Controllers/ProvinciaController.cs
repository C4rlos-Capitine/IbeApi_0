using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IbeApi.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProvinciaController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<ProvinciaController> _logger;

        public ProvinciaController(IConfiguration configuration, ILogger<ProvinciaController> logger) {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Get() {
            
            String sql = "SELECT CODPROVI, PROVINCI FROM GBIPROVI";
            _logger.LogInformation("SQL command prepared: {sql}", sql);
            List<Provincia> provinciaList = new List<Provincia>();


            try
            {
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read()) {
                        var provincia = new Provincia();
                        provincia.codprovi = reader.GetInt32(reader.GetOrdinal("CODPROVI"));
                        provincia.provinc = reader.GetString(reader.GetOrdinal("PROVINCI"));
                        provinciaList.Add(provincia);
                    }
                    return Ok(provinciaList);
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while fetching edital");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching candidate data: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching candidate data");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred: " + ex.Message);
            }


            return Ok();
        }
    }
}
