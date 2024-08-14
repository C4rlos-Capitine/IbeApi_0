using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IbeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocController : ControllerBase
    {






        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromQuery] string id, IFormFile file)
        {
            // Validate the ID parameter
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { StatusCode = 400, Message = "ID parameter is required." });
            }

            // Validate the file
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { StatusCode = 400, Message = "Ficheiro não enviado." });
            }

            try
            {
                // Define the path where the file will be saved, including the ID in the folder name
                var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", id);

                // Ensure the directory exists
                Directory.CreateDirectory(uploadsFolderPath);

                // Define the full path for the file
                var filePath = Path.Combine(uploadsFolderPath, file.FileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return success response with status code 201
                return Created(new Uri(filePath, UriKind.Relative), new
                {
                    StatusCode = 201,
                    Message = "Ficheiro emviado com sucesso.",
                    FilePath = filePath
                });
            }
            catch (Exception ex)
            {
                // Log the exception (you might want to log this to a file or monitoring system)
                Console.Error.WriteLine(ex);

                // Return error response with status code 500
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Um erro ocorreu ao enviar, contacte o IBE."
                });
            }
        }


    }


}
