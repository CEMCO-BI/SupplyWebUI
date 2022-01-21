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
    public class PlannedBuyImporter : Importer
    {
        public PlannedBuyImporter()
        {
            _hostingEnvironment = new HostingEnvironment { EnvironmentName = Environments.Development };
        }

        public static void RegisterImporter()
        {
            ImportService.RegisterImporter(Enums.FileNames.F_03, typeof(PlannedBuyImporter));
        }

        public override ImportResult Import(IFormFile file)
        {
            PlannedBuy plannedBuy;
            

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
                            plannedBuy = new PlannedBuy
                            {
                                Year = (int)_reader.GetDouble(0),
                                Month = (int)_reader.GetDouble(1),
                                Location = _reader.GetString(2),
                                Amount = _reader.GetDouble(3)
                            };

                            var plannedBuyFromDatabase = DataContext.PlannedBuy
                                .Where(sf => sf.Year == plannedBuy.Year
                                && sf.Month == plannedBuy.Month
                                && sf.Location.Equals(plannedBuy.Location)).FirstOrDefault();

                            if (plannedBuyFromDatabase != null)
                            {
                                plannedBuyFromDatabase.Amount = plannedBuy.Amount;
                            }
                            else
                            {
                                DataContext.PlannedBuy.Add(plannedBuy);
                            }
                        }

                        int output = DataContext.SaveChanges();

                        if (output > 0)
                        {
                            _importResult.Successful = true;
                            _importResult.Message = "The Excel file has been successfully uploaded.";
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
