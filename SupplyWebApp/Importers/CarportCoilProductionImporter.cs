using ExcelDataReader;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using SupplyWebApp.Helpers;
using SupplyWebApp.Models;
using SupplyWebApp.Services;
using SupplyWebApp.Validator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Services
{
    public class CarportCoilProductionImporter: Importer
    {
        public CarportCoilProductionImporter()
        {
            _hostingEnvironment = new HostingEnvironment { EnvironmentName = Environments.Development };
        }

        public static void RegisterImporter()
        {
            ImportService.RegisterImporter(Enums.FileNames.F_05, typeof(CarportCoilProductionImporter));
        }

        public override string Import(IFormFile file)
        {
            CarportCoilProduction carportCoilProduction;
            string result = null;
            CarportCoilProductionValidateObj carportCoilProductionValidateObj;


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
                            if(_reader.ResultsCount > 1)
                            {
                                _importResult.Successful = false;
                                _importResult.Message = "Uploaded File should have only 1 tab.";
                                result = JsonConvert.SerializeObject(_importResult);
                                return result;
                            }
                        }
                        else
                        {
                            _importResult.Successful = false;
                            _importResult.Message = "The file format is not supported.";
                        }

                        AdvanceToDataRow();
                        var line = 1;

                        while (_reader.Read() && _reader.ResultsCount == 1)
                        {
                            object year, month, classCode, amount;
                            if (_reader.GetValue(0) == null) { year = ""; } else year = _reader.GetValue(0);
                            if (_reader.GetValue(1) == null) { month = ""; } else month = _reader.GetValue(1);
                            if (_reader.GetValue(2) == null) { classCode = ""; } else classCode = _reader.GetValue(2);
                            if (_reader.GetValue(3) == null) { amount = ""; } else amount = _reader.GetValue(3);

                            //----------------------------------------------------
                            string year_v = year.ToString();
                            string month_v = month.ToString();
                            string classCode_v = classCode.ToString();
                            string amount_v = amount.ToString();
                            carportCoilProductionValidateObj = new CarportCoilProductionValidateObj(year_v, month_v, classCode_v, amount_v);
                            //step3:pass each row for validation.
                            // each object is passed to Validator for validation. 
                            CarportCoilProductionValidator sfv = new CarportCoilProductionValidator();
                            var results = sfv.Validate(carportCoilProductionValidateObj);

                            if (results.IsValid == false)
                            {

                                foreach (ValidationFailure failure in results.Errors)
                                {
                                    ValidationError v_error = new ValidationError(line, failure.ErrorMessage, carportCoilProductionValidateObj);
                                    _importResult.ErrorList.Add(v_error);
                                }
                            }


                            //-------------------------------------------------------


                            if (!_importResult.ErrorList.Any())
                            {

                                carportCoilProduction = new CarportCoilProduction
                                {
                                    Year = Convert.ToInt32(_reader?.GetValue(0)),
                                    Month = Convert.ToInt32(_reader?.GetValue(1)),
                                    ClassCode = Convert.ToString(_reader?.GetValue(2)),
                                    Amount = Convert.ToDouble(_reader?.GetValue(3))
                                };
                                

                                var carportCoilProductionFromDatabase = DataContext.CarportCoilProductions
                               .Where(sf => sf.Year == carportCoilProduction.Year
                               && sf.Month == carportCoilProduction.Month
                               && sf.ClassCode.Equals(carportCoilProduction.ClassCode)).FirstOrDefault();

                                if (carportCoilProductionFromDatabase != null)
                                {
                                    carportCoilProductionFromDatabase.Amount = carportCoilProduction.Amount;
                                }
                                else
                                {
                                    DataContext.CarportCoilProductions.Add(carportCoilProduction);
                                }

                                int output = DataContext.SaveChanges();


                                if (output > 0)
                                {

                                    _importResult.Successful = true;
                                    _importResult.Message = "The Excel file has been successfully uploaded.";
                                    Console.WriteLine("output :" + output);
                                }

                                result = JsonConvert.SerializeObject(_importResult);

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
    }
}
