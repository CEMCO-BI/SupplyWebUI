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
            _fileImporter = new ImportService(context);
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload()
        {
            //ImportResult importResult = null;
            string importResult = null;

            var file = Request.Form.Files[0];
            string typeOfFile = Request.Query["typeOfFile"].ToString();


            GlobalVars.FromDate = Request.Query["from"].ToString();
            GlobalVars.ToDate = Request.Query["to"].ToString();

            switch (typeOfFile)
            {
                // same import method to validate or to upload.
                case "F_01":
                    // this should take json data as return value.
                    importResult = _fileImporter.Import(Enums.FileNames.F_01, file);
                    break;
                case "F_02":
                    importResult = _fileImporter.Import(Enums.FileNames.F_02, file);
                    break;
                case "F_03":
                    importResult = _fileImporter.Import(Enums.FileNames.F_03, file);
                    break;
            }

            return Ok(importResult);
        }

        [HttpGet]
        [Route("/GetAddedFreightsDetails")]
        public IQueryable<AddedFreight> GetAddedFreightsDetails()
        {
            try
            {

                IQueryable<AddedFreight> addedFreightsFromdb = _dataContext.AddedFreight.AsQueryable();
                return addedFreightsFromdb;

            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("/GetTransferFreightsDetails")]
        public IQueryable<TransferFreight> GetTransferFreightsDetails()
        {
            try
            {

                IQueryable<TransferFreight> transferFreightsFromdb = _dataContext.TransferFreight.AsQueryable();
                return transferFreightsFromdb;

            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("/GetClassCodeManagementDetails")]
        public IQueryable<ClassCodeManagement> GetClassCodeManagementDetails()
        {
            try
            {

                IQueryable<ClassCodeManagement> classCodeManagementFromdb = _dataContext.ClassCodeManagement.AsQueryable();
                return classCodeManagementFromdb;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("/GetDisplayMonthsDetails")]
        public IQueryable<DisplayMonths> GetDisplayMonthsDetails()
        {
            try
            {

                IQueryable<DisplayMonths> displayMonthsFromdb = _dataContext.DisplayMonths.AsQueryable();
                return displayMonthsFromdb;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("/PostAddedFreightsDetails")]
        public async Task<IActionResult> PostAddedFreightsDetails(AddedFreight addedFreight)
        {
            string message = "";
            if (addedFreight != null)
            {
                try
                {
                    _dataContext.AddedFreight.Add(addedFreight);
                    int result = await _dataContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        message = "Added Freight records has been successfully added";
                    }
                    else
                    {
                        message = "Added Freight records insertion failed";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return Ok(message);
        }

    }
}