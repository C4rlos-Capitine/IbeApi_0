using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IbeApi.Models;
using System.Data.SqlClient;
using System.Data;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensagensController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<MensagensController> _logger;
        public MensagensController(IConfiguration configuration, ILogger<MensagensController> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }
        [HttpGet("{email}")]
        public IActionResult GetMsg(string email)
        {
            Mensagens mensagens = new Mensagens();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    const string sql = "SELECT ID, EMAIL, CODEDITAL, MSG, TITLE, LIDA, DATAENVIO FROM GBIMSG WHERE EMAIL = @EMAIL AND LIDA = @LIDA";
                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EMAIL", email);
                        command.Parameters.AddWithValue("@LIDA", 0);

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return NotFound("No messages found for the provided email.");
                            }

                            while (reader.Read())
                            {
                                mensagens.id = reader.GetInt32(reader.GetOrdinal("ID"));
                                mensagens.title = reader.GetString(reader.GetOrdinal("TITLE"));
                                mensagens.msg = reader.GetString(reader.GetOrdinal("MSG"));
                                mensagens.lida = reader.GetInt32(reader.GetOrdinal("LIDA"));
                                mensagens.data_envio = reader.GetDateTime(reader.GetOrdinal("DATAENVIO"));
                                mensagens.codedital = reader.GetInt32(reader.GetOrdinal("CODEDITAL"));
                                mensagens.email = reader.GetString(reader.GetOrdinal("EMAIL"));
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

            return Ok(mensagens);
        }

        [HttpGet("msg/search")]
        public IActionResult GetAllMsg([FromQuery] string email, [FromQuery] int all)
        {
            List<MensagensDTO> mensagens = new List<MensagensDTO>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    const string sql = "SELECT ID, EMAIL, CODEDITAL, MSG, TITLE, LIDA, NOME, DATAENVIO FROM GBIMSG  \r\n JOIN GBIEDITA ON GBIMSG.CODEDITAL = GBIEDITA.CODEDITA WHERE EMAIL = @EMAIL";
                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EMAIL", email);
                        command.Parameters.AddWithValue("@LIDA", 0);

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return NotFound("No messages found for the provided email.");
                            }

                            while (reader.Read())
                            {
                                MensagensDTO mensagem = new MensagensDTO();
                                mensagem.id = reader.GetInt32(reader.GetOrdinal("ID"));
                                mensagem.title = reader.GetString(reader.GetOrdinal("TITLE"));
                                mensagem.msg = reader.GetString(reader.GetOrdinal("MSG"));
                                mensagem.lida = reader.GetInt32(reader.GetOrdinal("LIDA"));
                                mensagem.data_envio = reader.GetDateTime(reader.GetOrdinal("DATAENVIO"));
                                mensagem.codedital = reader.GetInt32(reader.GetOrdinal("CODEDITAL"));
                                mensagem.email = reader.GetString(reader.GetOrdinal("EMAIL"));
                                mensagem.edital = reader.GetString(reader.GetOrdinal("NOME"));
                                mensagens.Add(mensagem);

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

            return Ok(mensagens);
        }

        [HttpPost]
        public IActionResult SaveMsg([FromBody] Mensagens mensagens)
        {
            //Mensagens mensagens = new Mensagens();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.LogInformation("Database connection opened.");



                    const string sql = @"
                INSERT INTO GBIMSG (EMAIL, CODEDITAL, MSG, TITLE, LIDA, DATAENVIO)
                OUTPUT INSERTED.EMAIL
                VALUES (@EMAIL, @CODEDITAL, @MSG, @TITLE, @LIDA, @DATAENVIO);";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@EMAIL", mensagens.email);
                        command.Parameters.AddWithValue("@TITLE", mensagens.title);
                        command.Parameters.AddWithValue("@MSG", mensagens.msg);
                        command.Parameters.AddWithValue("@LIDA", 0);
                        command.Parameters.AddWithValue("@CODEDITAL", mensagens.codedital);
                        command.Parameters.AddWithValue("@DATAENVIO", mensagens.data_envio);
                        mensagens.id = (int)command.ExecuteScalar();
                        _logger.LogInformation("mensagem guardada ID");
                    }
                }
                // Retornar uma resposta com código 201 e a mensagem de sucesso com o ID do candidato
                var result = new
                {
                    Message = "CandidatO registadO Com sucesso",
                    Code = mensagens.id,
                    success = true,
                };
                return Ok(result);
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

        [HttpPut("{id}")]
        public IActionResult MarkAsRead(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    const string sql = "UPDATE GBIMSG SET LIDA = @LIDA WHERE ID = @ID";
                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LIDA", 1); // Marcando como lida
                        command.Parameters.AddWithValue("@ID", id);

                        int rowsAffected = command.ExecuteNonQuery();
                
                        if (rowsAffected == 0)
                        {
                            return NotFound("No message found with the provided ID.");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL Error occurred while updating message status");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the message: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating message status");
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred: " + ex.Message);
            }

            return NoContent(); // Retorna 204 No Content se a atualização for bem-sucedida
        }

    }
}
