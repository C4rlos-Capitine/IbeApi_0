using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {

        private readonly string _connectionString;

        public FileUploadController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromQuery] string id, IFormFile file)
        {
            // Validate the ID parameter
            
           /* if (id == null)
            {
                return BadRequest(new { StatusCode = 400, Message = "ID parameter is required and must be greater than zero." });
            }

            // Validate the file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { StatusCode = 400, Message = "Ficheiro não enviado." });
            }*/

            try
            {
                // Read the file content into a byte array
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                // Update the database
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = "UPDATE GBICANDI SET CERTIFICADO = @CERTIFICADO WHERE CODCANDI = @CODCANDI";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CERTIFICADO", fileBytes);
                        command.Parameters.AddWithValue("@CODCANDI", id);

                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { StatusCode = 404, Message = "Registro não encontrado." });
                        }
                    }
                }

                // Return success response with status code 201
                return Created(string.Empty, new
                {
                    StatusCode = 201,
                    Message = "Ficheiro enviado com sucesso."
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine(ex);

                // Return error response with status code 500
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Um erro ocorreu ao enviar, contacte o IBE. " + ex.Message
                });
            }
        }
        [HttpGet("download")]
        public async Task<IActionResult> GetFile([FromQuery] int id)
        {
            // Validate the ID parameter
            if (id <= 0)
            {
                return BadRequest(new { StatusCode = 400, Message = "ID parameter is required and must be greater than zero." });
            }

            try
            {
                byte[] fileBytes = null;

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Correct query to select the file data from the database
                    var query = "SELECT CERTIFICADO FROM GBICANDI WHERE CODCANDI = @CODCANDI";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Add the correct parameter for the query
                        command.Parameters.AddWithValue("@CODCANDI", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Read the binary data from the CERTIFICADO column
                                fileBytes = reader["CERTIFICADO"] as byte[];

                                if (fileBytes == null || fileBytes.Length == 0)
                                {
                                    return NotFound(new { StatusCode = 404, Message = "Arquivo não encontrado." });
                                }
                            }
                            else
                            {
                                return NotFound(new { StatusCode = 404, Message = "Registro não encontrado." });
                            }
                        }
                    }
                }

                // Define the content type and filename for PDF
                const string contentType = "application/pdf";
                // Provide a meaningful file name if possible
                const string fileName = "downloaded-file.pdf";

                // Return the file as a download
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine(ex);

                // Return error response with status code 500
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Um erro ocorreu ao tentar baixar o arquivo, contacte o IBE. " + ex.Message
                });
            }
        }
    }
    
}
