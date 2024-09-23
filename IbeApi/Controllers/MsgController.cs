using IbeApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MsgController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<MsgController> _logger;
        public MsgController(IConfiguration configuration, ILogger<MsgController> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }
        [HttpGet("{email}")]
        public IActionResult GetAllMsg(string email)
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
    }
}
