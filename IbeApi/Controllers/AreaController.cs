using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IbeApi.Models;
using System.Data.SqlClient;


namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreaController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<AreaController> _logger;
        public AreaController(IConfiguration configuration, ILogger<AreaController> logger) 
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }

        [HttpGet("codedita")]
        public IActionResult get(int codedita)
        {
            var areas = new List<Area>();
            try
            {
                // Initialize the SqlConnection with the connection string
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    const string sql = "SELECT CODEDITA, CODAREA, AREAS FROM GBIAREA WHERE CODEDITA = @CODEDITA";
                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODEDITA", codedita);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var area = new Area
                                {
                                    codarea = reader.GetInt32(reader.GetOrdinal("CODAREA")),
                                    codedita = reader.GetInt32(reader.GetOrdinal("CODEDITA")),
                                    nome = reader.GetString(reader.GetOrdinal("AREAS"))
                                };

                                // Add the edital to the list
                                areas.Add(area);
                            }
                        }
                    }
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

            return Ok(areas);

        }
    }
}
