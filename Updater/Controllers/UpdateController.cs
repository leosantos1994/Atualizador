using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Updater.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {

        [HttpGet("{client}")]
        public async Task<ActionResult> Get(string client)
        {
            var response = new MidModel.ServiceModel()
            {
                HasUpdate = true,
                Id = new Guid(),
                IsPool = true,
                IsService = false,
                SiteUser = "brconselhos",
                SitePass = "a123",
                Name = "BRC2",
            };
            return Ok(response);
        }

        [HttpGet("Download")]
        public async Task<ActionResult> DownloadFile(Guid id)
        {
            string filePath = @"C:\backup\BRC2-BKP-28-08-2022-11-37-21.zip";
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType, Path.GetFileName(filePath));
        }

        
    }
}
