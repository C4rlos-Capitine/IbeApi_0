﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IbeApi.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

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

            CandidatoDTO candidato = new CandidatoDTO();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    const string sql = "SELECT CODCANDI, NOME, DATADENA, DATAEMIS, VALIDO, APELIDO, NOMECOMP, EMAIL, GENERO, TELEFONE, TELEMOVE, OCUPACAO, NATURALI, RUA, ESTADODO, GBIPROVI.CODPROVI, GBIPROVI.PROVINCI FROM GBICANDI JOIN GBIPROVI ON GBIPROVI.CODPROVI = GBICANDI.CODPROVI WHERE CODCANDI = @CODCANDI";

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
                                candidato.telefone = reader.GetString(reader.GetOrdinal("TELEFONE"));
                                candidato.telemovel = reader.GetString(reader.GetOrdinal("TELEMOVE"));
                                candidato.genero = reader.GetString(reader.GetOrdinal("GENERO"));
                                candidato.estado = reader.IsDBNull(reader.GetOrdinal("ESTADODO")) ? null : reader.GetString(reader.GetOrdinal("ESTADODO"));
                                candidato.ocupacao = reader.IsDBNull(reader.GetOrdinal("OCUPACAO")) ? null: reader.GetString(reader.GetOrdinal("OCUPACAO"));
                                candidato.naturalidade = reader.IsDBNull(reader.GetOrdinal("NATURALI")) ? null: reader.GetString(reader.GetOrdinal("NATURALI"));
                                candidato.rua = reader.IsDBNull(reader.GetOrdinal("RUA")) ? null : reader.GetString(reader.GetOrdinal("RUA"));
                                candidato.datadena = reader.GetDateTime("DATADENA");
                                candidato.data_emissao = reader.GetDateTime("DATAEMIS");
                                candidato.data_validade = reader.GetDateTime("VALIDO");
                                candidato.provincia = reader.IsDBNull(reader.GetOrdinal("PROVINCI")) ? null : reader.GetString(reader.GetOrdinal("PROVINCI"));
                                candidato.codprovi = reader.GetInt32(reader.GetOrdinal("CODPROVI"));
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

            CandidatoDTO candidato = new CandidatoDTO
            {
                FindTrue = false // Default to false
            };

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    const string sql = "SELECT CODCANDI, NOME, DATADENA, DATAEMIS, VALIDO, IDADE, APELIDO, NUMEO, NOMECOMP, EMAIL, TELEFONE, TELEMOVE, GENERO, ESTADODO, OCUPACAO, NATURALI, RUA, GBIPROVI.CODPROVI, GBIPROVI.PROVINCI FROM GBICANDI JOIN GBIPROVI ON GBIPROVI.CODPROVI = GBICANDI.CODPROVI WHERE EMAIL = @EMAIL AND PASSWORD = @PASSWORD";

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
                                candidato.telefone = reader.IsDBNull(reader.GetOrdinal("TELEFONE")) ? null : reader.GetString(reader.GetOrdinal("TELEFONE"));
                                candidato.telemovel = reader.IsDBNull(reader.GetOrdinal("TELEMOVE")) ? null : reader.GetString(reader.GetOrdinal("TELEMOVE"));
                                candidato.num_ident = reader.IsDBNull(reader.GetOrdinal("NUMEO")) ? 0 : reader.GetInt64(reader.GetOrdinal("NUMEO"));
                                candidato.genero = reader.IsDBNull(reader.GetOrdinal("GENERO")) ? null : reader.GetString(reader.GetOrdinal("GENERO"));
                                candidato.idade = reader.IsDBNull(reader.GetOrdinal("IDADE")) ? 0 : reader.GetInt16(reader.GetOrdinal("IDADE"));
                                candidato.estado = reader.IsDBNull(reader.GetOrdinal("ESTADODO")) ? null : reader.GetString(reader.GetOrdinal("ESTADODO"));
                                candidato.ocupacao = reader.IsDBNull(reader.GetOrdinal("OCUPACAO")) ? null : reader.GetString(reader.GetOrdinal("OCUPACAO"));
                                candidato.naturalidade = reader.IsDBNull(reader.GetOrdinal("NATURALI")) ? null : reader.GetString(reader.GetOrdinal("NATURALI"));
                                candidato.rua = reader.IsDBNull(reader.GetOrdinal("RUA")) ? null : reader.GetString(reader.GetOrdinal("RUA"));
                                candidato.datadena = reader.GetDateTime("DATADENA");
                                candidato.data_emissao = reader.GetDateTime("DATAEMIS");
                                candidato.data_validade = reader.GetDateTime("VALIDO");
                                candidato.provincia = reader.IsDBNull(reader.GetOrdinal("PROVINCI")) ? null : reader.GetString(reader.GetOrdinal("PROVINCI"));
                                candidato.codprovi = reader.GetInt32(reader.GetOrdinal("CODPROVI"));
                                candidato.FindTrue = true; // Set to true if candidate is found

                                _logger.LogInformation("Candidate data retrieved successfully for email {email} and phone {telefone}", email, password);
                            }
                            else
                            {
                                _logger.LogWarning("Candidate with email {email} and phone {telefone} not found.", email, password);
                                return NotFound(candidato);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while fetching candidate data for email {email} and phone {telefone}", email, password);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching candidate data: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching candidate data for email {email} and phone {telefone}", email, password);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred: " + ex.Message);
            }

            return Ok(candidato);
        }



        [HttpPost]
        public IActionResult Post([FromBody] Candidato candidato)
        {
            if (candidato == null)
            {
                _logger.LogWarning("Post request received with null candidate.");
                return BadRequest("Candidate data is null");
            }

            // Verificar se o NUMEO ou EMAIL já existem
            if (IsCandidatoExists(candidato.num_ident, candidato.email))
            {
                _logger.LogWarning(" NUMEO {numeo} or EMAIL {email} already exists.", candidato.num_ident, candidato.email);
                return Conflict("BI ou email já existente.");
            }

            int codcandi = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);

            _logger.LogInformation("Post request received for candidate: {candidato}");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");



                    const string sql = @"
                INSERT INTO GBICANDI (CODCANDI, CODPROVI, PASSWORD, NOME, APELIDO, NOMECOMP, NUMEO, EMAIL, TELEFONE, TELEMOVE, GENERO, DATADENA, IDADE, OCUPACAO, NATURALI, RUA, DATAEMIS, VALIDO, CODEDITA, CODAREA, NIVEL, ESPECIAL)
                OUTPUT INSERTED.CODCANDI
                VALUES (@CODCANDI, @CODPROVI, @PASSWORD, @NOME, @APELIDO, @NOMECOMP, @NUMEO, @EMAIL, @TELEFONE, @TELEMOVE, @GENERO, @DATADENA, @IDADE, @OCUPACAO, @NATURALI, @RUA, @DATAEMIS, @VALIDO, @CODEDITA, @CODAREA, @NIVEL, @ESPECIAL);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                        command.Parameters.AddWithValue("@CODPROVI", candidato.codprovi);
                        command.Parameters.AddWithValue("@NOME", (object)candidato.nome ?? DBNull.Value);
                        command.Parameters.AddWithValue("@APELIDO", (object)candidato.apelido ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PASSWORD", (object)candidato.password ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NOMECOMP", (object)candidato.nomecomp ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EMAIL", (object)candidato.email ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEFONE", (object)candidato.telefone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEMOVE", (object)candidato.telemovel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@GENERO", (object)candidato.genero ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DATADENA", new DateTime(candidato.ano, candidato.mes, candidato.dia));
                        command.Parameters.AddWithValue("@DATAEMIS", new DateTime(candidato.ano_emissao, candidato.mes_emissao, candidato.dia_emissao));
                        command.Parameters.AddWithValue("@VALIDO", new DateTime(candidato.ano_validade, candidato.mes_validade, candidato.dia_validade));
                        command.Parameters.AddWithValue("@NUMEO", (object)candidato.num_ident ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDADE", (object)candidato.idade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@OCUPACAO", (object)candidato.ocupacao ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NATURALI", (object)candidato.naturalidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RUA", (object)candidato.rua ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODEDITA", (object)candidato.codedital ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODAREA", (object)candidato.codarea ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ESPECIAL", (object)candidato.especialidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NIVEL", (object)candidato.nivel ?? DBNull.Value);



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
        /*
        [HttpPost]
        public IActionResult Post([FromBody] Candidato candidato)
        {
            if (candidato == null)
            {
                _logger.LogWarning("Post request received with null candidate.");
                return BadRequest("Candidate data is null");
            }

            // Verificar se o NUMEO ou EMAIL já existem
            if (IsCandidatoExists(candidato.num_ident, candidato.email))
            {
                _logger.LogWarning("NUMEO {numeo} or EMAIL {email} already exists.", candidato.num_ident, candidato.email);
                return Conflict("BI ou email já existente.");
            }

            int codcandi = BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0);

            _logger.LogInformation("Post request received for candidate: {candidato}");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    // Inserir na tabela GBICANDI
                    const string insertCandidatoSql = @"
                INSERT INTO GBICANDI (CODCANDI, CODPROVI, PASSWORD, NOME, APELIDO, NOMECOMP, NUMEO, EMAIL, TELEFONE, TELEMOVE, GENERO, DATADENA, IDADE, OCUPACAO, NATURALI, RUA, DATAEMIS, VALIDO, CODEDITA, CODAREA, NIVEL, ESPECIAL)
                OUTPUT INSERTED.CODCANDI
                VALUES (@CODCANDI, @CODPROVI, @PASSWORD, @NOME, @APELIDO, @NOMECOMP, @NUMEO, @EMAIL, @TELEFONE, @TELEMOVE, @GENERO, @DATADENA, @IDADE, @OCUPACAO, @NATURALI, @RUA, @DATAEMIS, @VALIDO, @CODEDITA, @CODAREA, @NIVEL, @ESPECIAL);";

                    using (var command = new SqlCommand(insertCandidatoSql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                        command.Parameters.AddWithValue("@CODPROVI", candidato.codprovi);
                        command.Parameters.AddWithValue("@NOME", (object)candidato.nome ?? DBNull.Value);
                        command.Parameters.AddWithValue("@APELIDO", (object)candidato.apelido ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PASSWORD", (object)candidato.password ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NOMECOMP", (object)candidato.nomecomp ?? DBNull.Value);
                        command.Parameters.AddWithValue("@EMAIL", (object)candidato.email ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEFONE", (object)candidato.telefone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEMOVE", (object)candidato.telemovel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@GENERO", (object)candidato.genero ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DATADENA", new DateTime(candidato.ano, candidato.mes, candidato.dia));
                        command.Parameters.AddWithValue("@DATAEMIS", new DateTime(candidato.ano_emissao, candidato.mes_emissao, candidato.dia_emissao));
                        command.Parameters.AddWithValue("@VALIDO", new DateTime(candidato.ano_validade, candidato.mes_validade, candidato.dia_validade));
                        command.Parameters.AddWithValue("@NUMEO", (object)candidato.num_ident ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IDADE", (object)candidato.idade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@OCUPACAO", (object)candidato.ocupacao ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NATURALI", (object)candidato.naturalidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RUA", (object)candidato.rua ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODEDITA", (object)candidato.codedital ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODAREA", (object)candidato.codarea ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ESPECIAL", (object)candidato.especialidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NIVEL", (object)candidato.nivel ?? DBNull.Value);

                        // Executar o comando e obter o ID do candidato inserido
                        candidato.codcandi = (int)command.ExecuteScalar();
                        _logger.LogInformation("Candidate data inserted successfully with ID {codcandi}", candidato.codcandi);
                    }

                    // Inserir na tabela GBICANDIDATURAS
                    const string insertCandidatoDetalheSql = @"
                INSERT INTO GBICANDIDATURAS (CODCANDI, CODEDITA, CODCURSO, DATASUBM, ESTADO, RESULTADO)
                VALUES (@CODCANDI, @CODEDITA, @CODCURSO, @DATASUBM, @ESTADO, @RESULTADO);";

                    using (var command = new SqlCommand(insertCandidatoDetalheSql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", candidato.codcandi);
                        command.Parameters.AddWithValue("@CODEDITA", (object)candidato.codedital ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CODCURSO", 1); // Supondo que o campo existe em Candidato
                        command.Parameters.AddWithValue("@DATASUBM", DateTime.UtcNow); // Supondo que o campo existe em Candidato
                        command.Parameters.AddWithValue("@ESTADO", "submetido"); // Supondo que o campo existe em Candidato
                        command.Parameters.AddWithValue("@RESULTADO", "Não disponivél"); // Supondo que o campo existe em Candidato

                        command.ExecuteNonQuery();
                        _logger.LogInformation("Candidate details inserted into GBICANDIDATURAS.");
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
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while inserting candidate data: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred: " + ex.Message);
            }
        }
        */
        private bool IsCandidatoExists(long numeo, string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                const string sql = @"
            SELECT COUNT(*)
            FROM GBICANDI
            WHERE NUMEO = @NUMEO OR EMAIL = @EMAIL;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@NUMEO", (object)numeo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EMAIL", (object)email ?? DBNull.Value);

                    var count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        [HttpPut("{codcandi}")]
        public IActionResult Put(int codcandi, [FromBody] CandidatoDTO candidato)
        {
            if (candidato == null)
            {
                _logger.LogWarning("Put request received with null candidate.");
                return BadRequest("Candidate data is null");
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");

                    // SQL query for updating candidate data, excluding password and email
                    const string sql = @"
                        UPDATE GBICANDI
                        SET CODPROVI = @CODPROVI,

                            TELEFONE = @TELEFONE,
                            TELEMOVE = @TELEMOVE,
                            NUMEO = @NUMEO,
                            OCUPACAO = @OCUPACAO,
                            NATURALI = @NATURALI,
                            RUA = @RUA
                        WHERE CODCANDI = @CODCANDI;
                        ";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@CODCANDI", codcandi);
                        command.Parameters.AddWithValue("@CODPROVI", candidato.codprovi);
                        command.Parameters.AddWithValue("@TELEFONE", (object)candidato.telefone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@TELEMOVE", (object)candidato.telemovel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NUMEO", (object)candidato.num_ident ?? DBNull.Value);
                        command.Parameters.AddWithValue("@OCUPACAO", (object)candidato.ocupacao ?? DBNull.Value);
                        command.Parameters.AddWithValue("@NATURALI", (object)candidato.naturalidade ?? DBNull.Value);
                        command.Parameters.AddWithValue("@RUA", (object)candidato.rua ?? DBNull.Value);

                        // Execute the command
                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            _logger.LogInformation("Candidate data updated successfully for ID {id}", codcandi);
                        }
                        else
                        {
                            _logger.LogWarning("No candidate data updated for ID {id}", codcandi);
                            return NotFound($"Candidate with ID {codcandi} not found.");
                        }
                    }
                }

                // Return a response indicating success
                var result = new
                {
                    Message = "Candidate data updated successfully",
                    Code = codcandi,
                    success = true,
                };

                return Ok(result);
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while updating candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating candidate data" + sqlEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating candidate data.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred" + ex);
            }
        }


    }
}
