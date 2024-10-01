using IbeApi.Models;
using IbeApi.Services;
//using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<MailController> _logger;
        IMailService Mail_Service = null;
        //injecting the IMailService into the constructor
        public MailController(IMailService _MailService, IConfiguration configuration, ILogger<MailController> logger)
        {
            Mail_Service = _MailService;
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }
        [HttpGet("email")]
        public IActionResult CheckEmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Email parameter is null or empty.");
                return BadRequest("Email cannot be null or empty.");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                _logger.LogInformation("Database connection opened.");

                const string sql = @"
            SELECT NOME FROM GBICANDI WHERE EMAIL = @EMAIL;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EMAIL", email);

                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string nome = reader.IsDBNull(reader.GetOrdinal("NOME")) ? null : reader.GetString(reader.GetOrdinal("NOME"));
                            _logger.LogInformation("Candidate data retrieved successfully for {email}", email);
                            return Ok(new { exists = true, name = nome });
                        }
                        else
                        {
                            _logger.LogWarning("Candidate with email {email} not found.", email);
                            return Ok(new { exists = false });
                        }
                    }
                }
            }
        }

        

        private bool emailExists(string email)
        {

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                const string sql = @"
            SELECT COUNT(*)
            FROM GBICANDI WHERE EMAIL = @EMAIL;";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EMAIL", (object)email ?? DBNull.Value);

                    var count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private int generateCode(string email)
        {
            Random random = new Random();
            int cod = -1;
            using (var connection = new SqlConnection(_connectionString))
            {
              
                connection.Open();
                const string sql = @"INSERT INTO MOBILE_AUTH (EMAIL, DATAGERACAO, CODIGO, AUTENTICOU) OUTPUT INSERTED.EMAIL VALUES (@EMAIL, @DATAGERACAO, @CODIGO, @AUTENTICOU);";
                cod = random.Next(100000, 1000000);
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EMAIL", email);
                    command.Parameters.AddWithValue("@DATAGERACAO", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@CODIGO", cod);
                    command.Parameters.AddWithValue("@AUTENTICOU", 0);
                    command.ExecuteScalar();
                    _logger.LogInformation("mensagem guardada ID");
                }
            }
            return cod;
        }

        [HttpPost]
        public bool SendMail(MailData Mail_Data)
        {
            if(Mail_Data.auth == 1)
            {
                int codigoGerado = generateCode(Mail_Data.EmailToId);
                Mail_Data.setCodigo(codigoGerado);
            }
            return Mail_Service.SendMail(Mail_Data);
        }
    }
}
