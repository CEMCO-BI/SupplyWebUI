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
    public class PlannedBuyImporter : ImportService
    {
        public PlannedBuyImporter()
        {
            _hostingEnvironment = new HostingEnvironment { EnvironmentName = Environments.Development };
        }

        public static void RegisterImporter()
        {
            FileImporter.RegisterImporter(Enums.FileNames.F_03, typeof(PlannedBuyImporter));
        }

        public override void Import(IFormFile file)
        {
            PlannedBuy plannedBuy;
            string message = "";

            try
            {
                if (file != null && file.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);

                        stream.Position = 0;

                        if (file.FileName.EndsWith(Constants.FILE_EXTENSION_XLS))
                        {
                            _reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        }
                        else if (file.FileName.EndsWith(Constants.FILE_EXTENSION_XLSX))
                        {
                            _reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        }
                        else
                        {
                            message = "The file format is not supported.";
                        }

                        AdvanceToDataRow();

                        while (_reader.Read())
                        {
                            plannedBuy = new PlannedBuy
                            {
                                Year = (int)_reader.GetDouble(0),
                                Month = (int)_reader.GetDouble(1),
                                Location = _reader.GetString(2)
                            };

                            var salesForecastFromDatabase = DataContext.SalesForecast
                                .Where(sf => sf.Year == plannedBuy.Year
                                && sf.Month == plannedBuy.Month
                                && sf.Location.Equals(plannedBuy.Location)).FirstOrDefault();

                            if (salesForecastFromDatabase != null)
                            {
                                salesForecastFromDatabase.Amount = plannedBuy.Amount;
                            }
                            else
                            {
                                DataContext.PlannedBuy.Add(plannedBuy);
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
                    }
                }
                else
                {
                    message = "Invalid or Empty File.";
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
