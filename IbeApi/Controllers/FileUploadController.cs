﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace IbeApi.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly string _storagePath;
        private readonly string _connectionString;

        public FileUploadController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServerDb");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromQuery] string id, int tipo, IFormFile file)
        {
           

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
                    var value = "@CERTIFICADO";
                    if (tipo == 1)
                    {
                        query = "UPDATE GBICANDI SET BI = @BI WHERE CODCANDI = @CODCANDI";
                        value = "@BI";
                    }else if(tipo == 2)
                    {
                        query = "UPDATE GBICANDI SET CERTIFICADO = @CERTIFICADO WHERE CODCANDI = @CODCANDI";
                        value = "@CERTIFICADO";
                    }else if (tipo == 3)
                    {
                        query = "UPDATE GBICANDI SET NUIT_DOC = @NUIT_DOC WHERE CODCANDI = @CODCANDI";
                        value = "@NUIT_DOC";
                    }else if (tipo == 4)
                    {
                        query = "UPDATE GBICANDI SET FOTOPASSE = @FOTOPASSE WHERE CODCANDI = @CODCANDI";
                        value = "@FOTOPASSE";
                    }
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(value, fileBytes);
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
        [HttpGet("download_certificado")]
        public async Task<IActionResult> GetCertificado([FromQuery] int id)
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
                const string fileName = "certificado-file.pdf";

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


        [HttpGet("download_identificacao")]
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
                    var query = "SELECT BI FROM GBICANDI WHERE CODCANDI = @CODCANDI";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Add the correct parameter for the query
                        command.Parameters.AddWithValue("@CODCANDI", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Read the binary data from the CERTIFICADO column
                                fileBytes = reader["BI"] as byte[];

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
                const string fileName = "identificacao.pdf";

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



        [HttpGet("download_nuit")]
        public async Task<IActionResult> GetNuit([FromQuery] int id)
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
                    var query = "SELECT NUIT FROM GBICANDI WHERE CODCANDI = @CODCANDI";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Add the correct parameter for the query
                        command.Parameters.AddWithValue("@CODCANDI", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Read the binary data from the CERTIFICADO column
                                fileBytes = reader["NUIT"] as byte[];

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
                const string fileName = "nuit.pdf";

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

        [HttpGet("download_foto")]
        public async Task<IActionResult> GetFoto([FromQuery] int id)
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
                    var query = "SELECT FOTOPASSE FROM GBICANDI WHERE CODCANDI = @CODCANDI";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Add the correct parameter for the query
                        command.Parameters.AddWithValue("@CODCANDI", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Read the binary data from the CERTIFICADO column
                                fileBytes = reader["FOTOPASSE"] as byte[];

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
                const string contentType = "application/png";
                // Provide a meaningful file name if possible
                const string fileName = "FOTO.png";

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
