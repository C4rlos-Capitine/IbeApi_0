using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace IbeApi.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        [HttpGet("paises/{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/paises", imageName, "img.png");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "image/png"); // Adjust MIME type as necessary
        }
    }
}
