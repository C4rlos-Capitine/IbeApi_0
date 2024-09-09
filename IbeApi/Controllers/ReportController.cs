using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;

namespace PdfGeneraterApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        [HttpGet("factura")]
        public async Task<IActionResult> Factura(String id)
        {
            ViewAsPdf vista = new ViewAsPdf();
            byte[] dados = await vista.BuildFile(ControllerContext);
            return new FileContentResult(dados, "application/pdf") { 
                FileDownloadName = "Factura.pdf"
            };
        }
    }
}
