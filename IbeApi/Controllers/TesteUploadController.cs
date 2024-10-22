using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using IbeApi.Models;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TesteUploadController : ControllerBase
    {
        private readonly string _connectionString;

        public TesteUploadController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb") ?? ""; // Ensure it will never be null
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles([FromForm] Person person)
        {
            if (person.FileA == null || person.FileB == null || string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Email))
            {
                return BadRequest("Please provide all required fields and files.");
            }

            // Convert files to byte arrays
            byte[] fileAData;
            using (var memoryStream = new MemoryStream())
            {
                await person.FileA.CopyToAsync(memoryStream);
                fileAData = memoryStream.ToArray();
            }

            byte[] fileBData;
            using (var memoryStream = new MemoryStream())
            {
                await person.FileB.CopyToAsync(memoryStream);
                fileBData = memoryStream.ToArray();
            }

            // Save to the database using SqlClient
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("INSERT INTO PERSON (NAME, EMAIL, FILE_A, FILE_B) OUTPUT INSERTED.ID VALUES (@Name, @Email, @FileA, @FileB)", connection);
                command.Parameters.Add(new SqlParameter("@Name", SqlDbType.VarChar) { Value = person.Name });
                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar) { Value = person.Email });
                command.Parameters.Add(new SqlParameter("@FileA", SqlDbType.VarBinary) { Value = fileAData });
                command.Parameters.Add(new SqlParameter("@FileB", SqlDbType.VarBinary) { Value = fileBData });

                var personId = await command.ExecuteScalarAsync();

                return Ok(new { message = "Files uploaded successfully!", personId });
            }
        }
    }
}
