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
using Microsoft.AspNetCore.Http;

namespace SupplyWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {
        private DataContext _dataContext;
        private ImportService _fileImporter;

        public FileUploadController(DataContext context)
        {
            _dataContext = context;
            _fileImporter =  new ImportService(context);
        }

        [HttpPost]
        [Route("upload")]
        public IActionResult Upload()
        {
            ImportResult importResult = null;

            var file = Request.Form.Files[0];

            string typeOfFile = Request.Query["typeOfFile"].ToString();

            GlobalVars.FromDate = Request.Query["from"].ToString();
            GlobalVars.ToDate = Request.Query["to"].ToString();

            switch (typeOfFile)
            {
                case "F_01":
                Console.WriteLine("---------F01----------");
                    importResult = _fileImporter.Import(Enums.FileNames.F_01, file);
                    break;
                case "F_02":
                    importResult = _fileImporter.Import(Enums.FileNames.F_02, file);
                    break;
                case "F_03":
                    importResult = _fileImporter.Import(Enums.FileNames.F_03, file);
                    break;
            }

            if (!importResult.Successful)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, importResult);
            }
            else
            {
                return Ok(importResult);
            }

        }
    }
}
