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
    public class CursoController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<CursoController> _logger;
        public CursoController(IConfiguration configuration, ILogger<CursoController> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Index() {
            List<Curso> cursos = new List<Curso>();
            var connection = new SqlConnection(_connectionString);
            String sql = "SELECT CODCURSO, CURSOS FROM GBICURSO";
            _logger.LogInformation("SQL command prepared: {sql}", sql);
            try
            {
                connection.Open();
                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var curso = new Curso();
                            curso.codcurso = reader.GetInt32(reader.GetOrdinal("CODCURSO"));
                            curso.nome = reader.GetString(reader.GetOrdinal("CURSOS"));
                            cursos.Add(curso);
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

            return Ok(cursos);
        }
    }
}
