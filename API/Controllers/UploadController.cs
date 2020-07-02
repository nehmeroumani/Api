using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    // [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public UploadController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpPost]
        [Route("files")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var id = Guid.NewGuid();
            var ext = Path.GetExtension(file.FileName).ToLower();
            string path = Path.Combine(_hostingEnvironment.WebRootPath, "attachments");
            path = Path.Combine(path, id + ext);
            if (file.Length > 0)
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }
            }
            return Ok(new { path, id = id + ext, ext= ext.Substring(1) });
        }

        //[HttpPost("files")]
        //public async Task<IActionResult> Post([FromForm]List<IFormFile> files)
        //{
        //    long size = files.Sum(f => f.Length);

        //    // full path to file in temp location
        //    var filePath = Path.GetTempFileName();

        //    foreach (var formFile in files)
        //    {
        //        if (formFile.Length > 0)
        //        {
        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await formFile.CopyToAsync(stream);
        //            }
        //        }
        //    }

        //    // process uploaded files
        //    // Don't rely on or trust the FileName property without validation.

        //    return Ok(new { count = files.Count, size, filePath });
        //}
        //[HttpPost("files")]
        //public IActionResult Files()
        //{
        //    long size = 0;
        //    var files = Request.Form.Files;
        //    foreach (var file in files)
        //    {
        //        var filename = ContentDispositionHeaderValue
        //            .Parse(file.ContentDisposition)
        //            .FileName
        //            .Trim('"');
        //        filename = $@"C:\{filename}";
        //        size += file.Length;
        //        using (FileStream fs = System.IO.File.Create(filename))
        //        {
        //            file.CopyTo(fs);
        //            fs.Flush();
        //        }
        //    }
        //    string message = $"{files.Count} file(s) / { size} bytes uploaded successfully!";
        //    return Ok(message);
        //}
    }
}
