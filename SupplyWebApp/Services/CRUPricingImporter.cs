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
    public class CRUPricingImporter : ImportService
    {
        public CRUPricingImporter()
        {
            _hostingEnvironment = new HostingEnvironment { EnvironmentName = Environments.Development };
        }

        public static void RegisterImporter()
        {
            FileImporter.RegisterImporter(Enums.FileNames.F_02, typeof(CRUPricingImporter));
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

                                CRUPricing cruPricing;

                                for (int i = 8; i < dtSalesForecast.Rows.Count; i++)
                                {

                                    cruPricing = new CRUPricing
                                    {
                                        Date = Convert.ToDateTime(dtSalesForecast.Rows[i][0]),
                                        Week1 = Convert.ToInt64(string.IsNullOrWhiteSpace(dtSalesForecast.Rows[i][1].ToString().Replace("-", "")) ? 0 : dtSalesForecast.Rows[i][1]),
                                        Week2 = Convert.ToInt64(string.IsNullOrWhiteSpace(dtSalesForecast.Rows[i][2].ToString().Replace("-", "")) ? 0 : dtSalesForecast.Rows[i][2]),
                                        Week3 = Convert.ToInt64(string.IsNullOrWhiteSpace(dtSalesForecast.Rows[i][3].ToString().Replace("-", "")) ? 0 : dtSalesForecast.Rows[i][3]),
                                        Week4 = Convert.ToInt64(string.IsNullOrWhiteSpace(dtSalesForecast.Rows[i][4].ToString().Replace("-", "")) ? 0 : dtSalesForecast.Rows[i][4]),
                                        Week5 = Convert.ToInt64(string.IsNullOrWhiteSpace(dtSalesForecast.Rows[i][5].ToString().Replace("-", "")) ? 0 : dtSalesForecast.Rows[i][5])
                                    };

                                    Console.WriteLine(cruPricing.Date);

                                    var cruPricingFromDatabase = DataContext.CRUPricing
                                        .Where(sf => sf.Date == cruPricing.Date).FirstOrDefault();

                                    if (cruPricingFromDatabase != null)
                                    {
                                        cruPricingFromDatabase.Week1 = cruPricing.Week1;
                                        cruPricingFromDatabase.Week2 = cruPricing.Week2;
                                        cruPricingFromDatabase.Week3 = cruPricing.Week3;
                                        cruPricingFromDatabase.Week4 = cruPricing.Week4;
                                        cruPricingFromDatabase.Week5 = cruPricing.Week5;
                                    }
                                    else
                                    {
                                        DataContext.CRUPricing.Add(cruPricing);
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
