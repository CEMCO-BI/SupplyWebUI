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

namespace SupplyWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileUploadController : ControllerBase
    {
        private IHostingEnvironment _hostingEnvironment;
        private DataContext _dataContext;
        public FileUploadController(IHostingEnvironment hostingEnvironment, DataContext context)
        {
            _hostingEnvironment = hostingEnvironment;
            _dataContext = context;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload()
        {
            string message = "";

            var formCollection = await Request.ReadFormAsync();
            DataSet dataSetFromExcel = new DataSet();
            IExcelDataReader reader = null;

            try
            {
                var file = Request.Form.Files[0];
                string path = Path.Combine(_hostingEnvironment.WebRootPath, Constants.FILE_UPLOAD_FOLDER);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (file.Length > 0)
                {
                    string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    string fullPath = Path.Combine(path, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);

                        if (file != null)
                        {
                            if (file.FileName.EndsWith(".xls"))
                            {
                                reader = ExcelReaderFactory.CreateBinaryReader(stream);
                            }
                            else if (file.FileName.EndsWith(".xlsx"))
                            {
                                reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                            }
                            else
                            {
                                message = "The file format is not supported.";
                            }

                            dataSetFromExcel = reader.AsDataSet();
                            reader.Close();

                            if (dataSetFromExcel != null && dataSetFromExcel.Tables.Count > 0)
                            {
                                DataTable dtSalesForecast = dataSetFromExcel.Tables[0];

                                SalesForecast salesForecast;

                                for (int i = 2; i < dtSalesForecast.Rows.Count; i++)
                                {

                                    salesForecast = new SalesForecast
                                    {
                                        Year = Convert.ToInt32(dtSalesForecast.Rows[i][0]),
                                        Month = Convert.ToInt32(dtSalesForecast.Rows[i][1]),
                                        Location = Convert.ToString(dtSalesForecast.Rows[i][2]),
                                        Amount = Convert.ToInt64(dtSalesForecast.Rows[i][3])
                                    };

                                    var salesForecastFromDatabase = _dataContext.SalesForecast
                                        .Where(sf => sf.Year == salesForecast.Year
                                        && sf.Month == salesForecast.Month
                                        && sf.Location.Equals(salesForecast.Location)).FirstOrDefault();

                                    if (salesForecastFromDatabase != null)
                                    {
                                        salesForecastFromDatabase.Amount = salesForecast.Amount;
                                    }
                                    else
                                    {
                                        _dataContext.SalesForecast.Add(salesForecast);
                                    }
                                }

                                int output = _dataContext.SaveChanges();

                                if (output > 0)
                                {
                                    message = "The Excel file has been successfully uploaded.";
                                }
                                else
                                {
                                    message = "Something Went Wrong!, The Excel file uploaded has fiald.";
                                }
                            }
                            else
                            {
                                message = "Selected file is empty.";
                                //return HttpContext.Response.StatusCode = 500 ;
                            }
                        }
                        else
                        {
                            message = "Invalid File.";
                            return BadRequest();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return Ok();
        }
    }
}
