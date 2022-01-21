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
    public class CRUPricingImporter : Importer
    {
        public CRUPricingImporter()
        {
            _hostingEnvironment = new HostingEnvironment { EnvironmentName = Environments.Development };
            _dataStartRow = 9;
        }

        public static void RegisterImporter()
        {
            ImportService.RegisterImporter(Enums.FileNames.F_02, typeof(CRUPricingImporter));
        }

        public override ImportResult Import(IFormFile file)
        {
            CRUPricing cruPricing;
            string message = "";

            try
            {
                if (file != null && file.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);

                        stream.Position = 0;
                        if (file.FileName.EndsWith(".xls"))
                        {
                            _reader = ExcelReaderFactory.CreateBinaryReader(stream);
                        }
                        else if (file.FileName.EndsWith(".xlsx"))
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
                            cruPricing = new CRUPricing
                            {
                                Date = _reader.GetDateTime(0),
                                Week1 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(1)?.ToString()) ? "0" : _reader.GetValue(1).ToString().Replace("-", "0")),
                                Week2 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(2)?.ToString()) ? "0" : _reader.GetValue(2).ToString().Replace("-", "0")),
                                Week3 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(3)?.ToString()) ? "0" : _reader.GetValue(3).ToString().Replace("-", "0")),
                                Week4 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(4)?.ToString()) ? "0" : _reader.GetValue(4).ToString().Replace("-", "0")),
                                Week5 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(5)?.ToString()) ? "0" : _reader.GetValue(5).ToString().Replace("-", "0")),
                            };

                            if (cruPricing.Date >= Convert.ToDateTime(GlobalVars.FromDate) && cruPricing.Date <= Convert.ToDateTime(GlobalVars.ToDate))
                            {
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
                        }

                        int output = DataContext.SaveChanges();

                        if (output > 0)
                        {
                            message = "The Excel file has been successfully uploaded.";
                        }
                        else
                        {
                            message = "Something Went Wrong!, The Excel file uploaded has field.";
                        }
                    }
                }
                else
                {
                    message = "Invalid File.";
                }
            }
            catch (Exception ex)
            {
            }
            return _importResult;
        }
    }
}
