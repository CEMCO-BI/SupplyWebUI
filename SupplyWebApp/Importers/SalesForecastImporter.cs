﻿using ExcelDataReader;
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

//step1: take the file.
        public override string Import(IFormFile file)
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

//step4: store the errors from each row in a list.
                            if (results.IsValid == false)
                            {
                                
                                foreach (ValidationFailure failure in results.Errors)
                                {

                                    ValidationError v_error = new ValidationError(line, failure.ErrorMessage);
                                    _importResult.ErrorList.Add(v_error);
                                    
                                     
                                }
                            }

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

// ALL OF THE ERRORS:
// STORING: 1. ALL Errors   2.Successful(boolean)              3.Message. into result variable below.
                        string result = JsonConvert.SerializeObject(_importResult);
                        Console.WriteLine("------ALL ERRORS-----");
                        Console.WriteLine(result);

                        if( _importResult.ErrorList.Count() != 0){
                            return result;
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
                Console.WriteLine("catch block --------- "+ex.Message);
                _importResult.Successful = false;
                _importResult.Message = "Error occurred - " + ex.Message;
            }

            return result;
        }
    }
}
