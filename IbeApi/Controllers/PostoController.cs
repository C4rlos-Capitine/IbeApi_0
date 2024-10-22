using IbeApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostoController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<PostoController> _logger;
        public PostoController(IConfiguration configuration, ILogger<PostoController> logger) {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {

            String sql = "SELECT CODPOSTO, POSTOAD FROM GBIPOSTO";
            _logger.LogInformation("SQL command prepared: {sql}", sql);
            List<PostoAdministrativo> postoList = new List<PostoAdministrativo>();


            try
            {
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var posto = new PostoAdministrativo();
                        posto.codposto = reader.GetInt32(reader.GetOrdinal("CODPOSTO"));
                        posto.nome = reader.GetString(reader.GetOrdinal("POSTOAD"));
                        postoList.Add(posto);
                    }
                    return Ok(postoList);
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
