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

namespace SupplyWebApp.Services
{
    public class SalesForecastImporter : Importer
    {
        List<String> errors = new List<string>();
        IDictionary<int, List<String>> listOfErrors = new Dictionary<int, List<String>>();
        public SalesForecastImporter()
        {
            _hostingEnvironment = new HostingEnvironment { EnvironmentName = Environments.Development };
        }

        public static void RegisterImporter()
        {
            ImportService.RegisterImporter(Enums.FileNames.F_01, typeof(SalesForecastImporter));
        }

//step1: take the file.
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
                        var line = 2;
//step2: convert each row to object.                        
                        // here each row is converted into a salesforecast object.
                        while (_reader.Read())
                        {
                            Console.WriteLine("----------------------------");


                            salesForecast = new SalesForecast
                            {
                                Year = (int)_reader.GetDouble(0),
                                Month = (int)_reader.GetDouble(1),
                                Location = _reader.GetString(2),
                                Amount = _reader.GetDouble(3)
                            };
//step3:pass each row for validation.
                            // each object is passed to Validator for validation. 
                            SalesForecastValidator sfv = new SalesForecastValidator();
                            var results = sfv.Validate(salesForecast);

//step4: store the errors in each row in a list
                            if (results.IsValid == false)
                            {
                                foreach (ValidationFailure failure in results.Errors)
                                {
                                    // errors is a list. all generated errors are added to it.
                                    errors.Add(failure.ErrorMessage);
                                }
//step5: Add the line and the listOf Errors to a dictionary.                                 
                                listOfErrors.Add(line,errors);
                            }
                            //try printing the errors with specific row.
                            /*
                            foreach (var error in results.Errors)
                           {
                               Console.WriteLine(error + " =>"+line);
                           }
                            */



//step6: Pass this dictionary as JSON Object
                            /* if(!dictionary.isEmpty){

                                      return dictionary in json format
                              }*/







                            // code to check if data already exists.
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
                            line++;
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
                Console.WriteLine("catch block ---------");
                _importResult.Successful = false;
                _importResult.Message = "Error occurred - " + ex.Message;
            }

            return _importResult;
        }
    }
}
