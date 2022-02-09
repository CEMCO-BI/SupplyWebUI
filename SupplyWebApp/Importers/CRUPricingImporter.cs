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
using FluentValidation;
using System.ComponentModel;
using FluentValidation.Results;
using Newtonsoft.Json;
using SupplyWebApp.Models.TransferObjects;

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

        public override string Import(IFormFile file)
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
                        // May - 13  750 748 739 738 725

                        AdvanceToDataRow();
                        var line = 1;
                        while (_reader.Read())
                        {



                            //----------------------------------
                            object amount1, amount2, amount3, amount4, amount5;
                            object date = _reader.GetValue(0);
                            if (_reader.GetValue(1) == null) { amount1 = ""; } else amount1 = _reader.GetValue(1);
                            if (_reader.GetValue(2) == null) { amount2 = ""; } else amount2 = _reader.GetValue(2);
                            if (_reader.GetValue(3) == null) { amount3 = ""; } else amount3 = _reader.GetValue(3);
                            if (_reader.GetValue(4) == null) { amount4 = ""; } else amount4 = _reader.GetValue(4);
                            if (_reader.GetValue(5) == null) { amount5 = ""; } else amount5 = _reader.GetValue(5);


                            string date_v = date.ToString();
                            string week1_v = amount1.ToString();
                            string week2_v = amount2.ToString();
                            string week3_v = amount3.ToString();
                            string week4_v = amount4.ToString();
                            string week5_v = amount5.ToString();

                            CRUPricingValidateObj cruPricingValidateObj = new CRUPricingValidateObj(date_v,week1_v, week2_v, week3_v, week4_v, week5_v);
                            CRUPricingValidator cruv = new CRUPricingValidator();
                            var results = cruv.Validate(cruPricingValidateObj);

                            if (results.IsValid == false)
                            {

                                foreach (ValidationFailure failure in results.Errors)
                                {
                                    ValidationError v_error = new ValidationError(line, failure.ErrorMessage, cruPricingValidateObj);
                                    _importResult.ErrorList.Add(v_error);
                                }
                            }
                            // all the errors in a row are stored in a list. 
                            if (!_importResult.ErrorList.Any())
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

                               
                            line++;
                        }

                        if (!_importResult.ErrorList.Any())
                        {
                            int output = DataContext.SaveChanges();

                            if (output > 0)
                            {

                                _importResult.Successful = true;
                                _importResult.Message = "The Excel file has been successfully uploaded.";
                            }

                        }
                    }
                }
                else
                {
                    _importResult.Successful = false;
                    _importResult.Message = "Invalid or Empty File.";
                }

                if (_importResult.ErrorList.Any())
                {
                    _importResult.Successful = false;
                    _importResult.Message = "There are some errors in the File.";
                    result = JsonConvert.SerializeObject(_importResult);
                    //return result;

                }
                result = JsonConvert.SerializeObject(_importResult);
                return result;
            }
            catch (Exception ex)
            {
                _importResult.Successful = false;
                _importResult.Message = "Error occurred - " + ex.Message;
                Console.WriteLine(ex.Message+"------");
                return result;
            }
            
        }
    }
}
