using IbeApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistritoController2 : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<DistritoController2> _logger;
        public DistritoController2(IConfiguration configuration, ILogger<DistritoController2> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }
        [HttpGet("codprovi")]
        public IActionResult GetByProvince(int cod)
        {
            String sql = "SELECT CODDISTR, DISTRITO FROM GBIDISTR WHERE CODPROVI = @CODPROVI ORDER BY (DISTRITO)";
            _logger.LogInformation("SQL command prepared: {sql}", sql);
            List<Distrito> distritoList = new List<Distrito>();


            try
            {
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CODPROVI", cod);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var distrito = new Distrito();
                        distrito.coddistrito = reader.GetInt32(reader.GetOrdinal("CODDISTR"));
                        distrito.nome = reader.GetString(reader.GetOrdinal("DISTRITO"));
                        distritoList.Add(distrito);
                    }
                    return Ok(distritoList);
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
        }
    }
}
