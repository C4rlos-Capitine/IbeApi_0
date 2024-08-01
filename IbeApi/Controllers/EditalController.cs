using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IbeApi.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditalController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<EditalController> _logger;

        public EditalController(IConfiguration configuration, ILogger<EditalController> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetEditais()
        {
            //Edital edital = new Edital();
            var editais = new List<Edital>();
            try
            {
                // Initialize the SqlConnection with the connection string
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    const string sql = "SELECT CODEDITA, ANO, NUMERO, NOME FROM GBIEDITA";
                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var edital = new Edital
                                {
                                    codEdital = reader.GetInt32(reader.GetOrdinal("CODEDITA")),
                                    numero = reader.GetInt32(reader.GetOrdinal("NUMERO")),
                                    ano = reader.GetInt16(reader.GetOrdinal("ANO")),
                                    nome = reader.GetString(reader.GetOrdinal("NOME"))
                                };

                                // Add the edital to the list
                                editais.Add(edital);
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

            return Ok(editais);
        }
    }
}
