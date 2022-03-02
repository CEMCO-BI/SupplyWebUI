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

        public override string Import(IFormFile file)
        {
            PlannedBuy plannedBuy;
            string result = null;
            PlannedBuyValidateObj plannedBuyValidateObj;

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
                        var line = 1;

                        while (_reader.Read())
                        {
                            object year = _reader.GetValue(0);
                            object month = _reader.GetValue(1);
                            object location = _reader.GetValue(2);
                            object amount = _reader.GetValue(3);
                            object cwt = _reader.GetValue(4);



                            string year_v = year.ToString();
                            string month_v = month.ToString();
                            string location_v = location.ToString();
                            string amount_v = amount.ToString();
                            string cwt_v = cwt.ToString();
                            Console.WriteLine("cwt value  :" + cwt_v);
                            Console.WriteLine("-------------------------");
                            Console.WriteLine("-------------------------");
                            Console.WriteLine("-------------------------");
                            Console.WriteLine("-------------------------");

                            plannedBuyValidateObj = new PlannedBuyValidateObj(year_v, month_v, location_v, amount_v,cwt_v);

                            PlannedBuyValidator pbv = new PlannedBuyValidator();
                            var results = pbv.Validate(plannedBuyValidateObj);


                            if (results.IsValid == false)
                            {

                                foreach (ValidationFailure failure in results.Errors)
                                {
                                    ValidationError v_error = new ValidationError(line, failure.ErrorMessage, plannedBuyValidateObj);
                                    _importResult.ErrorList.Add(v_error);
                                }
                            }


                            if (!_importResult.ErrorList.Any())
                            {

                                plannedBuy = new PlannedBuy
                                {
                                    Year = Convert.ToInt32(_reader.GetValue(0)),
                                    Month = Convert.ToInt32(_reader.GetValue(1)),
                                    Location = _reader.GetString(2),
                                    Amount = Convert.ToDouble(_reader.GetValue(3)),
                                    Cwt = Convert.ToDouble(_reader.GetValue(4))
                                };

                                var plannedBuyFromDatabase = DataContext.PlannedBuy
                                 .Where(sf => sf.Year == plannedBuy.Year
                                 && sf.Month == plannedBuy.Month
                                 && sf.Location.Equals(plannedBuy.Location)).FirstOrDefault();


                                if (plannedBuyFromDatabase != null)
                                {
                                    plannedBuyFromDatabase.Amount = plannedBuy.Amount;
                                    plannedBuyFromDatabase.Cwt = plannedBuy.Cwt;
                                }
                                else
                                {
                                    DataContext.PlannedBuy.Add(plannedBuy);
                                }
                                int output = DataContext.SaveChanges();


                                if (output > 0)
                                {

                                    _importResult.Successful = true;
                                    _importResult.Message = "The Excel file has been successfully uploaded.";
                                    Console.WriteLine("output :" + output);
                                }

                                result = JsonConvert.SerializeObject(_importResult);
                                Console.WriteLine("result   :" + result);
                            }
                            line++;
                        }
                        if (_importResult.ErrorList.Any())
                        {
                            _importResult.Successful = false;
                            _importResult.Message = "There are some errors in the File.";
                            result = JsonConvert.SerializeObject(_importResult);
                            //return result;

                        }
                    }




                }
                else
                {
                    _importResult.Successful = false;
                    _importResult.Message = "Invalid or Empty File.";
                    result = JsonConvert.SerializeObject(_importResult);
                }

                return result;

            }
            catch (Exception ex)
            {

                _importResult.Successful = false;
                _importResult.Message = "Error occurred - " + ex.Message;
                result = JsonConvert.SerializeObject(_importResult);
                return result;
            }



        }
    } }