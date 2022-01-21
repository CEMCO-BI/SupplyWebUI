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
    public class SalesForecastImporter : Importer
    {
        public SalesForecastImporter()
        {
            _hostingEnvironment = new HostingEnvironment { EnvironmentName = Environments.Development };
        }

        public static void RegisterImporter()
        {
            ImportService.RegisterImporter(Enums.FileNames.F_01, typeof(SalesForecastImporter));
        }

        public override ImportResult Import(IFormFile file)
        {
            SalesForecast salesForecast;

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
                            _importResult.Successful = false;
                            _importResult.Message = "The file format is not supported.";
                        }

                        AdvanceToDataRow();

                        while (_reader.Read())
                        {
                            salesForecast = new SalesForecast
                            {
                                Year = (int)_reader.GetDouble(0),
                                Month = (int)_reader.GetDouble(1),
                                Location = _reader.GetString(2),
                                Amount = _reader.GetDouble(3)
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

                            int output = DataContext.SaveChanges();

                            if (output > 0)
                            {
                                _importResult.Successful = true;
                                _importResult.Message = "The Excel file has been successfully uploaded.";
                            }
                            else
                            {
                                _importResult.Successful = false;
                                _importResult.Message = "Something Went Wrong!, The Excel file uploaded has failed.";
                            }
                        }
                    }
                }
                else
                {
                    _importResult.Successful = false;
                    _importResult.Message = "Invalid or Empty File.";
                }
            }
            catch (Exception ex)
            {
                _importResult.Successful = false;
                _importResult.Message = "Error occurred - " + ex.Message;
            }

            return _importResult;
        }
    }
}
