using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using SupplyWebApp.Data;
using SupplyWebApp.Helpers;
using SupplyWebApp.Models;
using SupplyWebApp.Models.TransferObjects;
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
            CRUPricingTransferObject cruPricingTransferObject;

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
                            _importResult.Successful = false;
                            _importResult.Message = "The file format is not supported.";
                        }

                        AdvanceToDataRow();

                        while (_reader.Read())
                        {
                            cruPricingTransferObject = new CRUPricingTransferObject
                            {
                                Date = _reader.GetDateTime(0),
                                Week1 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(1)?.ToString()) ? "0" : _reader.GetValue(1).ToString().Replace("-", "0")),
                                Week2 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(2)?.ToString()) ? "0" : _reader.GetValue(2).ToString().Replace("-", "0")),
                                Week3 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(3)?.ToString()) ? "0" : _reader.GetValue(3).ToString().Replace("-", "0")),
                                Week4 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(4)?.ToString()) ? "0" : _reader.GetValue(4).ToString().Replace("-", "0")),
                                Week5 = Convert.ToDouble(string.IsNullOrWhiteSpace(_reader.GetValue(5)?.ToString()) ? "0" : _reader.GetValue(5).ToString().Replace("-", "0")),
                            };

                            if (cruPricingTransferObject.Date >= Convert.ToDateTime(GlobalVars.FromDate) && cruPricingTransferObject.Date <= Convert.ToDateTime(GlobalVars.ToDate))
                            {
                                var cruPricingsFromDatabase = DataContext.CRUPricing
                                .Where(sf => sf.Year == cruPricingTransferObject.Date.Year && sf.Month == cruPricingTransferObject.Date.Month);

                                if (cruPricingTransferObject.Week1 > 0)
                                {
                                    if (!cruPricingsFromDatabase.Any(x => x.Week == 1))
                                    {
                                        DataContext.CRUPricing.Add(new CRUPricing()
                                        {
                                            Amount = cruPricingTransferObject.Week1,
                                            Month = cruPricingTransferObject.Date.Month,
                                            Year = cruPricingTransferObject.Date.Year,
                                            Week = 1
                                        });
                                    }
                                    else
                                    {
                                        cruPricingsFromDatabase.FirstOrDefault(x => x.Week == 1).Amount = cruPricingTransferObject.Week1;
                                    }
                                }

                                if (cruPricingTransferObject.Week2 > 0)
                                {
                                    if (!cruPricingsFromDatabase.Any(x => x.Week == 2))
                                    {
                                        DataContext.CRUPricing.Add(new CRUPricing()
                                        {
                                            Amount = cruPricingTransferObject.Week2,
                                            Month = cruPricingTransferObject.Date.Month,
                                            Year = cruPricingTransferObject.Date.Year,
                                            Week = 2
                                        });
                                    }
                                    else
                                    {
                                        cruPricingsFromDatabase.FirstOrDefault(x => x.Week == 2).Amount = cruPricingTransferObject.Week2;
                                    }
                                }

                                if (cruPricingTransferObject.Week3 > 0)
                                {
                                    if (!cruPricingsFromDatabase.Any(x => x.Week == 3))
                                    {
                                        DataContext.CRUPricing.Add(new CRUPricing()
                                        {
                                            Amount = cruPricingTransferObject.Week3,
                                            Month = cruPricingTransferObject.Date.Month,
                                            Year = cruPricingTransferObject.Date.Year,
                                            Week = 3
                                        });
                                    }
                                    else
                                    {
                                        cruPricingsFromDatabase.FirstOrDefault(x => x.Week == 3).Amount = cruPricingTransferObject.Week3;
                                    }
                                }

                                if (cruPricingTransferObject.Week4 > 0)
                                {
                                    if (!cruPricingsFromDatabase.Any(x => x.Week == 4))
                                    {
                                        DataContext.CRUPricing.Add(new CRUPricing()
                                        {
                                            Amount = cruPricingTransferObject.Week4,
                                            Month = cruPricingTransferObject.Date.Month,
                                            Year = cruPricingTransferObject.Date.Year,
                                            Week = 4
                                        });
                                    }
                                    else
                                    {
                                        cruPricingsFromDatabase.FirstOrDefault(x => x.Week == 4).Amount = cruPricingTransferObject.Week4;
                                    }
                                }

                                if (cruPricingTransferObject.Week5 > 0)
                                {
                                    if (!cruPricingsFromDatabase.Any(x => x.Week == 5))
                                    {
                                        DataContext.CRUPricing.Add(new CRUPricing()
                                        {
                                            Amount = cruPricingTransferObject.Week5,
                                            Month = cruPricingTransferObject.Date.Month,
                                            Year = cruPricingTransferObject.Date.Year,
                                            Week = 5
                                        });
                                    }
                                    else
                                    {
                                        cruPricingsFromDatabase.FirstOrDefault(x => x.Week == 5).Amount = cruPricingTransferObject.Week5;
                                    }
                                }
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
