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
    public class CandidaturaController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<CandidaturaController> _logger;
        public CandidaturaController(IConfiguration configuration, ILogger<CandidaturaController> logger) {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }

        [HttpGet("{codcandi}")]
        public IActionResult Get(int codcandi)
        {
            List<Candidatura> candidaturas = new List<Candidatura>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string sql = "SELECT GBICANDIDATURAS.CODCANDI, GBIEDITA.CODEDITA, DATASUBM, GBICURSO.CODCURSO, DATASUBM, ESTADO, RESULTADO, CURSOS, GBIEDITA.NOME FROM GBICANDIDATURAS \r\nJOIN GBICANDI ON GBICANDIDATURAS.CODCANDI = GBICANDI.CODCANDI\r\nJOIN GBIEDITA ON GBICANDIDATURAS.CODEDITA = GBIEDITA.CODEDITA\r\nJOIN GBICURSO ON GBICANDIDATURAS.CODCURSO = GBICURSO.CODCURSO\r\nWHERE GBICANDIDATURAS.CODCANDI = @CODCANDI";
                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    using (var command = new SqlCommand(sql, connection))
                    {
                        // Add the parameter with its value
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var candidatura = new Candidatura();
                                candidatura.codcandi = reader.GetInt32(reader.GetOrdinal("CODCANDI"));
                                candidatura.cod_edital = reader.GetInt32(reader.GetOrdinal("CODEDITA"));
                                candidatura.codecurso = reader.GetInt32(reader.GetOrdinal("CODCURSO"));
                                candidatura.estado = reader.GetString(reader.GetOrdinal("ESTADO"));
                                candidatura.curso = reader.GetString(reader.GetOrdinal("CURSOS"));
                                candidatura.edital = reader.GetString(reader.GetOrdinal("NOME"));
                                candidatura.estado = reader.GetString(reader.GetOrdinal("ESTADO"));
                                candidatura.resultado = reader.GetString(reader.GetOrdinal("RESULTADO"));
                                candidatura.data_subm = reader.GetDateTime(reader.GetOrdinal("DATASUBM"));
                                candidaturas.Add(candidatura);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while fetching candidate data for ID {codcandi}", codcandi);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching candidate data: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching candidate data for ID {codcandi}", codcandi);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred: " + ex.Message);
            }

            return Ok(candidaturas);
        }

        [HttpPost]
        public IActionResult saveCandidatura([FromBody] CandidaturaDTO candidatura)
        {
            _logger.LogInformation("Post request received for candidate: {candidato}", candidatura);

            if (candidatura == null)
            {
                _logger.LogWarning("Post request received with null candidate.");
                return BadRequest("Candidate data is null");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    const string insertSql = @"
                        INSERT INTO GBICANDIDATURAS (CODCANDI, CODEDITA, CODCURSO, DATASUBM, ESTADO, RESULTADO)
                        VALUES (@CODCANDI, @CODEDITA, @CODCURSO, @DATASUBM, @ESTADO, @RESULTADO);";

                    const string updateSql = @"
                        UPDATE GBICANDI
                        SET CODCURSO = @CODCURSO, CODEDITA = @CODEDITA, ESTADODO = @ESTADODO
                        WHERE CODCANDI = @CODCANDI;";

                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var insertCommand = new SqlCommand(insertSql, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@CODCANDI", candidatura.codcandi);
                            insertCommand.Parameters.AddWithValue("@CODEDITA", candidatura.cod_edital);
                            insertCommand.Parameters.AddWithValue("@CODCURSO", candidatura.codecurso);
                            insertCommand.Parameters.AddWithValue("@DATASUBM", new DateTime(candidatura.ano_submissao, candidatura.mes_submissao, candidatura.dia_submissao));
                            insertCommand.Parameters.AddWithValue("@ESTADO", "SUBMETIDO");
                            insertCommand.Parameters.AddWithValue("@RESULTADO", "NÃO DISPONÍVEL");

                            // Execute the insert command
                            insertCommand.ExecuteNonQuery();
                            _logger.LogInformation("Candidate data inserted successfully with ID {codcandi}", candidatura.codcandi);
                        }

                        using (var updateCommand = new SqlCommand(updateSql, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@CODCANDI", candidatura.codcandi);
                            updateCommand.Parameters.AddWithValue("@CODEDITA", candidatura.cod_edital);
                            updateCommand.Parameters.AddWithValue("@CODCURSO", candidatura.codecurso);
                            updateCommand.Parameters.AddWithValue("@ESTADODO", "S");

                            // Execute the update command
                            updateCommand.ExecuteNonQuery();
                            _logger.LogInformation("Candidate data updated in GBICANDI with ID {codcandi}", candidatura.codcandi);
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                }

                // Return a response with status code 201 and a success message with the candidate ID
                var result = new
                {
                    Message = "Candidate registered successfully",
                    Code = candidatura.codcandi,
                    success = true,
                };

                return CreatedAtAction(nameof(Get), new { codcandi = candidatura.codcandi }, result);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while inserting candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while inserting candidate data: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred: " + ex.Message);
            }
        }




    }
}
