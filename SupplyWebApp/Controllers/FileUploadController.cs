using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SupplyWebApp.Data;
using SupplyWebApp.Models;
using System;
using System.Linq;
using System.Data;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SupplyWebApp.Helpers;
using SupplyWebApp.Services;

namespace SupplyWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {
        private DataContext _dataContext;

        public FileUploadController(DataContext context)
        {
            _dataContext = context;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload()
        {
            var file = Request.Form.Files[0];
            var formData = Request.Body;
            string message = "";

            FileImporter fileImporter = new FileImporter(_dataContext);

            fileImporter.Import(Enums.FileNames.F_01, file);

            return Ok();
        }
    }
}
