﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IbeApi.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatoController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<CandidatoController> _logger;

        public CandidatoController(IConfiguration configuration, ILogger<CandidatoController> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }

        [HttpGet("{codcandi}")]
        public IActionResult Get(int codcandi)
        {
            _logger.LogInformation("Get request received for candidate ID {codcandi}", codcandi);

            Candidato candidato = new Candidato();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    const string sql = "SELECT CODCANDI, NOME, APELIDO, NOMECOMP, EMAIL, GENERO, TELEFONE, TELEMOVE FROM GBICANDI WHERE CODCANDI = @CODCANDI";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                        _logger.LogInformation("SQL command prepared: {sql}", sql);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                candidato.codcandi = reader.GetInt32(reader.GetOrdinal("CODCANDI"));
                                candidato.nome = reader.GetString(reader.GetOrdinal("NOME"));
                                candidato.apelido = reader.GetString(reader.GetOrdinal("APELIDO"));
                                candidato.nomecomp = reader.GetString(reader.GetOrdinal("NOMECOMP"));
                                candidato.email = reader.GetString(reader.GetOrdinal("EMAIL"));
                                candidato.telefone = reader.GetInt64(reader.GetOrdinal("TELEFONE"));
                                candidato.telemovel = reader.GetInt64(reader.GetOrdinal("TELEMOVE"));
                                candidato.genero = reader.GetString(reader.GetOrdinal("GENERO"));

                                _logger.LogInformation("Candidate data retrieved successfully for ID {codcandi}", codcandi);
                            }
                            else
                            {
                                _logger.LogWarning("Candidate with ID {codcandi} not found.", codcandi);
                                return NotFound("Candidate not found");
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while fetching candidate data for ID {codcandi}", codcandi);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching candidate data"+sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching candidate data for ID {codcandi}", codcandi);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred"+ex);
            }

            return Ok(candidato);
        }

        [HttpGet("search")]
        public IActionResult GetByEmailAndPhone([FromQuery] string email, [FromQuery] string password)
        {
            _logger.LogInformation("Get request received for candidate with email {email} and phone {telefone}", email, password);

            Candidato candidato = new Candidato();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    const string sql = "SELECT CODCANDI, NOME, APELIDO, NOMECOMP, EMAIL, TELEFONE, TELEMOVE, GENERO FROM GBICANDI WHERE EMAIL = @EMAIL AND PASSWORD = @PASSWORD";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EMAIL", email);
                        command.Parameters.AddWithValue("@PASSWORD", password);
                        _logger.LogInformation("SQL command prepared: {sql}", sql);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                candidato.codcandi = reader.GetInt32(reader.GetOrdinal("CODCANDI"));
                                candidato.nome = reader.IsDBNull(reader.GetOrdinal("NOME")) ? null : reader.GetString(reader.GetOrdinal("NOME"));
                                candidato.apelido = reader.IsDBNull(reader.GetOrdinal("APELIDO")) ? null : reader.GetString(reader.GetOrdinal("APELIDO"));
                                candidato.nomecomp = reader.IsDBNull(reader.GetOrdinal("NOMECOMP")) ? null : reader.GetString(reader.GetOrdinal("NOMECOMP"));
                                candidato.email = reader.IsDBNull(reader.GetOrdinal("EMAIL")) ? null : reader.GetString(reader.GetOrdinal("EMAIL"));
                                candidato.telefone = reader.IsDBNull(reader.GetOrdinal("TELEFONE")) ? 0 : reader.GetInt64(reader.GetOrdinal("TELEFONE"));
                                candidato.telemovel = reader.IsDBNull(reader.GetOrdinal("TELEMOVE")) ? 0 : reader.GetInt64(reader.GetOrdinal("TELEMOVE"));


                                candidato.genero = reader.IsDBNull(reader.GetOrdinal("GENERO")) ? null : reader.GetString(reader.GetOrdinal("GENERO"));

                                _logger.LogInformation("Candidate data retrieved successfully for email {email} and phone {telefone}", email, password
                                    );
                            }
                            else
                            {
                                _logger.LogWarning("Candidate with email {email} and phone {telefone} not found.", email, password);
                                return NotFound("Candidate not found");
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while fetching candidate data for email {email} and phone {telefone}", email, password);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching candidate data"+sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching candidate data for email {email} and phone {telefone}", email, password);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred"+ex);
            }

            return Ok(candidato);
        }


        [HttpPost]
        public IActionResult Post([FromBody] Candidato candidato)
        {
            _logger.LogInformation("Post request received for candidate: {candidato}", candidato);

            if (candidato == null)
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

                    const string sql = @"
                INSERT INTO GBICANDI (CODCANDI, PASSWORD, NOME, APELIDO, NOMECOMP, EMAIL, TELEFONE, TELEMOVE, GENERO)
                OUTPUT INSERTED.CODCANDI
                VALUES (@CODCANDI, @PASSWORD, @NOME, @APELIDO, @NOMECOMP, @EMAIL, @TELEFONE, @TELEMOVE, @GENERO);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", (object)candidato.codcandi ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NOME", (object)candidato.nome ?? DBNull.Value);
                        command.Parameters.AddWithValue("@APELIDO", (object)candidato.apelido ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PASSWORD", (object)candidato.password ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NOMECOMP", (object)candidato.nomecomp ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EMAIL", (object)candidato.email ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEFONE", candidato.telefone != 0 ? (object)candidato.telefone : DBNull.Value);
                        command.Parameters.AddWithValue("@TELEMOVE", candidato.telemovel != 0 ? (object)candidato.telemovel : DBNull.Value);
                        command.Parameters.AddWithValue("@GENERO", (object)candidato.genero ?? DBNull.Value);

                        // Executar o comando e obter o ID do candidato inserido
                        candidato.codcandi = (int)command.ExecuteScalar();
                        _logger.LogInformation("Candidate data inserted successfully with ID {codcandi}", candidato.codcandi);
                    }
                }

                // Retornar uma resposta com código 201 e a mensagem de sucesso com o ID do candidato
                var result = new
                {
                    Message = "Candidate registered successfully",
                    Code = candidato.codcandi,
                    success = true,
                };

                return CreatedAtAction(nameof(Get), new { codcandi = candidato.codcandi }, result);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while inserting candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while inserting candidate data" + sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred" + ex);
            }
        }

    }
}