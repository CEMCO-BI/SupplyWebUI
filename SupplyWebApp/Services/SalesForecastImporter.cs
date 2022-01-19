using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using SupplyWebApp.Data;
using SupplyWebApp.Helpers;
using SupplyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace SupplyWebApp.Services
{
    public class SalesForecastImporter : ImportService
    {
        public SalesForecastImporter()
        {
            _hostingEnvironment = new HostingEnvironment { EnvironmentName = Environments.Development };
        }

        public static void RegisterImporter()
        {
            FileImporter.RegisterImporter(Enums.FileNames.F_01, typeof(SalesForecastImporter));
        }

        public override void Import(IFormFile file)
        {
            DataSet dataSetFromExcel = new DataSet();
            IExcelDataReader reader = null;
            string message = "";

            try
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.IndexOf("bin\\"))), Constants.FILE_UPLOAD_FOLDER);

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

                                    var salesForecastFromDatabase = DataContext.SalesForecast
                                        .Where(sf => sf.Year == salesForecast.Year
                                        && sf.Month == salesForecast.Month
                                        && sf.Location.Equals(salesForecast.Location)).FirstOrDefault();

                                    if (salesForecastFromDatabase != null)
                                    {
                                        salesForecastFromDatabase.Amount = salesForecast.Amount;
                                    }
                                    else
                                    {
                                        DataContext.SalesForecast.Add(salesForecast);
                                    }
                                }

                                int output = DataContext.SaveChanges();

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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
