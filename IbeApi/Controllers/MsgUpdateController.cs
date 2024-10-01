using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MsgUpdateController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<MsgController> _logger;
        public MsgUpdateController(IConfiguration configuration, ILogger<MsgController> logger)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
            _logger = logger;
        }
        [HttpPost("{email}")]
        public IActionResult MarkAsRead(String email)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    const string sql = "UPDATE GBIMSG SET LIDA = @LIDA WHERE @EMAIL = @EMAIL";
                    _logger.LogInformation("SQL command prepared: {sql}", sql);

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@LIDA", 1); // Marcando como lida
                        command.Parameters.AddWithValue("@EMAIL", email);

                        int rowsAffected = command.ExecuteNonQuery();
                        
                        if (rowsAffected == 0)
                        {
                            return NotFound("No message found with the provided ID.");
                        }
                        return new JsonResult(rowsAffected);
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

        }
    }
}
