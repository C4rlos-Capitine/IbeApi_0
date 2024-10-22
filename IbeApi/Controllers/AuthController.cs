using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger; 
        }

        [HttpGet("authenticate")]
        public IActionResult TryAuth(string email, int codigo)
        {
            // String de conexão ao seu banco de dados

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM MOBILE_AUTH WHERE EMAIL = @EMAIL AND CODIGO = @CODIGO AND EXPIROU = @EXPIROU";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Adiciona parâmetros para evitar SQL Injection
                    command.Parameters.AddWithValue("@EMAIL", email);
                    command.Parameters.AddWithValue("@CODIGO", codigo); 
                    command.Parameters.AddWithValue("@EXPIROU", 0);

                    var result = command.ExecuteScalar();

                    try
                    {
                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            return Ok(new
                            {   count_res = count,
                                success = true,
                                message = "Autenticação bem-sucedida"
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                count_res = count,
                                success = false,
                                message = "código incorretos ou expirou."
                            });
                        }
                    }
                    catch (SqlException ex)
                    {
                        return StatusCode(500, new { success = false, message = "Erro ao acessar o banco de dados.", error = ex.Message });
                    }
                }
            }
        }

        [HttpPost("setAuthenticated")]
        public IActionResult SetAuthenticated(string email)
        {

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE MOBILE_AUTH SET AUTENTICOU = 1 WHERE EMAIL = @EMAIL";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EMAIL", email);

                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok(new
                            {
                                success = true,
                                message = "Autenticação atualizada com sucesso."
                            });
                        }
                        else
                        {
                            return Ok(new
                            {
                                success = false,
                                message = "Nenhum registro encontrado para atualizar."
                            });
                        }
                    }
                    catch (SqlException ex)
                    {
                        return StatusCode(500, new { success = false, message = "Erro ao acessar o banco de dados.", error = ex.Message });
                    }
                }
            }
        }


    }
}
