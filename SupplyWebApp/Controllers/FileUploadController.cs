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
using Microsoft.EntityFrameworkCore;

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
                var addedFreightData = _dataContext.AddedFreight.Include(x=>x.Location)
                                        .Include(c=> c.Carrier)
                                        .Include(w => w.Warehouse)
                                        .Include(v => v.Vendor).AsQueryable();
                IQueryable<AddedFreight> addedFreightsFromdb = addedFreightData;
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
                var transferFreightsData = _dataContext.TransferFreight
                                            .Include(x => x.LocationFrom)
                                            .Include(x => x.LocationTo).AsQueryable();

                IQueryable<TransferFreight> transferFreightsFromdb = transferFreightsData;
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
                var classCodeManagementData = _dataContext.ClassCodeManagement
                                           .Include(x => x.Location)
                                           .Include(x => x.ClassCode)
                                           .Include(x => x.Part)
                                           .AsQueryable();
                IQueryable<ClassCodeManagement> classCodeManagementFromdb = classCodeManagementData;
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
                    transferFreight.TransferCost = Convert.ToDouble(transferFreightfromRequest[3].Value);
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

        [HttpPut]
        [Route("/UpdateAddedFreightDetails")]
        public async Task<IActionResult> UpdateAddedFreightDetails()
        {
            string message = "";
            var addedFreightFromReq = Request.Form.ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                AddedFreight addedFreight = new AddedFreight();
                addedFreight = _dataContext.AddedFreight.Find(Convert.ToInt32(addedFreightFromReq[0].Value));
                if (addedFreight != null)
                {
                    addedFreight.POLocationId = Convert.ToInt32(addedFreightFromReq[1].Value);
                    addedFreight.POWarehouseId = Convert.ToInt32(addedFreightFromReq[2].Value);
                    addedFreight.POCarrierId = Convert.ToInt32(addedFreightFromReq[3].Value);
                    addedFreight.VendorId = Convert.ToInt32(addedFreightFromReq[4].Value);
                    addedFreight.CWT = addedFreightFromReq[5].Value;
                    addedFreight.TruckLoad = addedFreightFromReq[6].Value;
                }
                _dataContext.AddedFreight.Update(addedFreight);
                int result = await _dataContext.SaveChangesAsync();
                if (result > 0)
                {
                    message = "Added Freight Record has been sussfully updated";
                }
                else
                {
                    message = "Added Freight Record updation Failed";
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return Ok(message);
        }

        [HttpPut]
        [Route("/UpdateTransferFreightDetails")]
        public async Task<IActionResult> UpdateTransferFreightDetails()
        {
            string message = "";
            var transferFreightFromReq = Request.Form.ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                TransferFreight transferFreight = new TransferFreight();
                transferFreight = _dataContext.TransferFreight.Find(Convert.ToInt32(transferFreightFromReq[0].Value));
                if (transferFreight != null)
                {
                    transferFreight.TransferFromId = Convert.ToInt32(transferFreightFromReq[1].Value);
                    transferFreight.TransferToId = Convert.ToInt32(transferFreightFromReq[2].Value);
                    transferFreight.ProductCode = transferFreightFromReq[3].Value;
                    transferFreight.TransferCost = Convert.ToDouble(transferFreightFromReq[4].Value);
                }
                _dataContext.TransferFreight.Update(transferFreight);
                int result = await _dataContext.SaveChangesAsync();
                if (result > 0)
                {
                    message = "Transfer Freight Record has been sussfully updated";
                }
                else
                {
                    message = "Transfer Freight Record updation Failed";
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return Ok(message);
        }

        [HttpPut]
        [Route("/UpdateClassCodeDetails")]
        public async Task<IActionResult> UpdateClassCodeDetails()
        {
            string message = "";
            var classCodesFromReq = Request.Form.ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                ClassCodeManagement classCodes = new ClassCodeManagement();
                classCodes = _dataContext.ClassCodeManagement.Find(Convert.ToInt32(classCodesFromReq[0].Value));
                if (classCodes != null)
                {
                    classCodes.ClassCodeID = Convert.ToInt32(classCodesFromReq[1].Value);
                    classCodes.ProductCodeId = Convert.ToInt32(classCodesFromReq[2].Value);
                    classCodes.LocationId = Convert.ToInt32(classCodesFromReq[3].Value);
                    classCodes.Active = Convert.ToInt32(classCodesFromReq[4].Value);
                }
                _dataContext.ClassCodeManagement.Update(classCodes);
                int result = await _dataContext.SaveChangesAsync();
                if (result > 0)
                {
                    message = "ClassCodeManagement Record has been sussfully updated";
                }
                else
                {
                    message = "ClassCodeManagement Record updation Failed";
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return Ok(message);
        }

        [HttpPut]
        [Route("/UpdateDisplayMonthsDetails")]
        public async Task<IActionResult> UpdateDisplayMonthsDetails()
        {
            string message = "";
            var displayMonthsFromReq = Request.Form.ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                DisplayMonths dm = new DisplayMonths();
                dm = _dataContext.DisplayMonths.Find(Convert.ToInt32(displayMonthsFromReq[0].Value));
                if (dm != null)
                {
                    dm.Month = Convert.ToInt32(displayMonthsFromReq[1].Value);
                    dm.Year = Convert.ToInt32(displayMonthsFromReq[2].Value);
                    dm.Active = Convert.ToInt32(displayMonthsFromReq[3].Value);
                }
                _dataContext.DisplayMonths.Update(dm);
                int result = await _dataContext.SaveChangesAsync();
                if (result > 0)
                {
                    message = "Display Months Record has been sussfully updated";
                }
                else
                {
                    message = "Display Months Record updation Failed";
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return Ok(message);
        }

        [HttpDelete]
        [Route("/DeleteAddedFreightRecord")]
        public async Task<IActionResult> DeleteAddedFreightRecord(int id)
        {
            string message = "";
            try
            {
                AddedFreight addedFreight = _dataContext.AddedFreight.Find(id);
                _dataContext.AddedFreight.Remove(addedFreight);
                int result = await _dataContext.SaveChangesAsync();
                if (result > 0)
                {
                    message = "Added Freight Record has been sucessfully deleted";
                }
                else
                {
                    message = "failed";
                }
            }
            catch (Exception e)
            {
                throw;
            }
            
            return Ok(message);
        }

        [HttpDelete]
        [Route("/DeleteTransferFreightRecord")]
        public async Task<IActionResult> DeleteTransferFreightRecord(int id)
        {
            string message = "";
            try
            {
                TransferFreight transferFreight = _dataContext.TransferFreight.Find(id);
                _dataContext.TransferFreight.Remove(transferFreight);
                int result = await _dataContext.SaveChangesAsync();
                if (result > 0)
                {
                    message = "Transfer Freight Record has been sucessfully deleted";
                }
                else
                {
                    message = "failed";
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return Ok(message);
        }

        [HttpDelete]
        [Route("/DeleteClassCodesRecord")]
        public async Task<IActionResult> DeleteClassCodesRecord(int id)
        {
            string message = "";
            try
            {
                ClassCodeManagement classCode = _dataContext.ClassCodeManagement.Find(id);
                _dataContext.ClassCodeManagement.Remove(classCode);
                int result = await _dataContext.SaveChangesAsync();
                if (result > 0)
                {
                    message = "Added Freight Record has been sucessfully deleted";
                }
                else
                {
                    message = "failed";
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return Ok(message);
        }

        [HttpDelete]
        [Route("/DeleteDisplayMonthsRecord")]
        public async Task<IActionResult> DeleteDisplayMonthsRecord(int id)
        {
            string message = "";
            try
            {
                DisplayMonths displayMonths = _dataContext.DisplayMonths.Find(id);
                _dataContext.DisplayMonths.Remove(displayMonths);
                int result = await _dataContext.SaveChangesAsync();
                if (result > 0)
                {
                    message = "Added Freight Record has been sucessfully deleted";
                }
                else
                {
                    message = "failed";
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return Ok(message);
        }
    }
}