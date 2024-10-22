using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly string _storagePath;
        private readonly string _connectionString;

        public UploadController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromQuery] string email, IFormFile bi, IFormFile nuit, IFormFile certificado, IFormFile foto)
        {
            try
            {
                // Cria um dicionário para armazenar os bytes dos arquivos
                var fileBytes = new Dictionary<string, byte[]>();

                // Lê os arquivos e armazena em bytes
                if (bi != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await bi.CopyToAsync(memoryStream);
                        fileBytes["BI"] = memoryStream.ToArray();
                    }
                }

                if (nuit != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await nuit.CopyToAsync(memoryStream);
                        fileBytes["NUIT"] = memoryStream.ToArray();
                    }
                }

                if (certificado != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await certificado.CopyToAsync(memoryStream);
                        fileBytes["CERTIFICADO"] = memoryStream.ToArray();
                    }
                }

                if (foto != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await foto.CopyToAsync(memoryStream);
                        fileBytes["FOTOPASSE"] = memoryStream.ToArray();
                    }
                }

                // Atualiza o banco de dados
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = "UPDATE GBICANDI SET ";
                    var setClauses = new List<string>();
                    var parameters = new List<SqlParameter>();

                    // Adiciona cláusulas SET para cada arquivo recebido
                    if (fileBytes.ContainsKey("BI"))
                    {
                        setClauses.Add("BI = @BI");
                        parameters.Add(new SqlParameter("@BI", fileBytes["BI"]));
                    }

                    if (fileBytes.ContainsKey("NUIT"))
                    {
                        setClauses.Add("NUIT_DOC = @NUIT_DOC");
                        parameters.Add(new SqlParameter("@NUIT_DOC", fileBytes["NUIT"]));
                    }

                   if (fileBytes.ContainsKey("CERTIFICADO"))
                    {
                        setClauses.Add("CERTIFICADO = @CERTIFICADO");
                        parameters.Add(new SqlParameter("@CERTIFICADO", fileBytes["CERTIFICADO"]));
                    }

                    if (fileBytes.ContainsKey("FOTOPASSE"))
                    {
                        setClauses.Add("FOTOPASSE = @FOTOPASSE");
                        parameters.Add(new SqlParameter("@FOTOPASSE", fileBytes["FOTOPASSE"]));
                    }

                    // Concatena as cláusulas SET e adiciona a cláusula WHERE
                    query += string.Join(", ", setClauses) + " WHERE EMAIL = @EMAIL";
                    Console.WriteLine(query);
                    parameters.Add(new SqlParameter("@EMAIL", email));

                    using (var command = new SqlCommand(query, connection))
                    {
                        //command.Parameters.AddRange(parameters.ToArray());
                        command.Parameters.AddWithValue("@EMAIL", email);
                        command.Parameters.AddWithValue("@BI", fileBytes["BI"]);
                        command.Parameters.AddWithValue("@NUIT_DOC", fileBytes["NUIT"]);
                        command.Parameters.AddWithValue("@CERTIFICADO", fileBytes["CERTIFICADO"]);
                        command.Parameters.AddWithValue("@FOTOPASSE", fileBytes["FOTOPASSE"]);
                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            return NotFound(new { StatusCode = 404, Message = "Registro não encontrado." });
                        }
                    }
                }

                // Retorna resposta de sucesso com status code 200
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Arquivos enviados com sucesso."
                });
            }
            catch (Exception ex)
            {
                // Loga a exceção
                Console.Error.WriteLine(ex);

                // Retorna resposta de erro com status code 500
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Um erro ocorreu ao enviar, contacte o IBE. " + ex.Message
                });
            }
        }

    }
}
