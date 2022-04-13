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

                //IQueryable<DisplayMonths> displayMonthsFromdb = _dataContext.DisplayMonths.AsQueryable();
                var res = _dataContext.DisplayMonths;
                return res;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("/PostAddedFreightsDetails")]
        public async Task<IActionResult> PostAddedFreightsDetails()
        {
            string message = "";
            var addedFreightfromRequest  = Request.Form.ToList();

            if (addedFreightfromRequest != null)
            {
                try
                {
                    AddedFreight addedFreight = new AddedFreight();
                    addedFreight.POLocationId = Convert.ToInt32(addedFreightfromRequest[0].Value);
                    addedFreight.POWarehouseId = Convert.ToInt32(addedFreightfromRequest[1].Value);
                    addedFreight.POCarrierId = Convert.ToInt32(addedFreightfromRequest[2].Value);
                    addedFreight.VendorId = Convert.ToInt32(addedFreightfromRequest[3].Value);
                    addedFreight.CWT = addedFreightfromRequest[4].Value;
                    addedFreight.TruckLoad = addedFreightfromRequest[5].Value;
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

        [HttpPost]
        [Route("/PostTransferFreightsDetails")]
        public async Task<IActionResult> PostTransferFreightsDetails()
        {
            string message = "";
            var transferFreightfromRequest = Request.Form.ToList();

            if (transferFreightfromRequest != null)
            {
                try
                {
                    TransferFreight transferFreight = new TransferFreight();
                    transferFreight.TransferFromId = Convert.ToInt32(transferFreightfromRequest[0].Value);
                    transferFreight.TransferToId = Convert.ToInt32(transferFreightfromRequest[1].Value);
                    transferFreight.ProductCode = transferFreightfromRequest[2].Value;
                    transferFreight.TransferCost = transferFreightfromRequest[3].Value;
                    _dataContext.TransferFreight.Add(transferFreight);
                    int result = await _dataContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        message = "Transfer Freight records has been successfully added";
                    }
                    else
                    {
                        message = "Transfer Freight records insertion failed";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return Ok(message);
        }

        [HttpPost]
        [Route("/PostClassCodesDetails")]
        public async Task<IActionResult> PostClassCodesDetails()
        {
            string message = "";
            var classCodesfromRequest = Request.Form.ToList();

            if (classCodesfromRequest != null)
            {
                try
                {
                    ClassCodeManagement classCodes = new ClassCodeManagement();
                    classCodes.ClassCodeID = Convert.ToInt32(classCodesfromRequest[0].Value);
                    classCodes.ProductCodeId = Convert.ToInt32(classCodesfromRequest[1].Value);
                    classCodes.LocationId = Convert.ToInt32(classCodesfromRequest[2].Value);
                    classCodes.Active = Convert.ToInt32(classCodesfromRequest[3].Value);
                    _dataContext.ClassCodeManagement.Add(classCodes);
                    int result = await _dataContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        message = "classCodes records has been successfully added";
                    }
                    else
                    {
                        message = "classCodes records insertion failed";
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
            }

            return Ok(message);
        }

        [HttpPost]
        [Route("/PostDisplayMonthsDetails")]
        public async Task<IActionResult> PostDisplayMonthsDetails()
        {
            string message = "";
            var displayMonthsfromRequest = Request.Form.ToList();

            if (displayMonthsfromRequest != null)
            {
                try
                {
                    DisplayMonths displayMonths = new DisplayMonths();
                    displayMonths.Month = Convert.ToInt32(displayMonthsfromRequest[0].Value);
                    displayMonths.Year = Convert.ToInt32(displayMonthsfromRequest[1].Value);
                    displayMonths.Active = Convert.ToInt32(displayMonthsfromRequest[2].Value);
                    _dataContext.DisplayMonths.Add(displayMonths);
                    int result = await _dataContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        message = "displayMonths records has been successfully added";
                    }
                    else
                    {
                        message = "displayMonths records insertion failed";
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