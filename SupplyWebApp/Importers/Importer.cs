using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SupplyWebApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SupplyWebApp.Helpers.Enums;
using Microsoft.Extensions.Configuration;
using ExcelDataReader;
using SupplyWebApp.Models;

namespace SupplyWebApp.Services
{
    public abstract class Importer
    {
        public DataContext DataContext;
        protected IHostEnvironment _hostingEnvironment;
        protected IExcelDataReader _reader;
        protected int _dataStartRow = 2;
        protected ImportResult _importResult = new ImportResult();

        public virtual ImportResult Import(IFormFile file)
        {
            return _importResult;
        }

        public void AdvanceToDataRow()
        {
            for(int i = 1; i < _dataStartRow; i++)
            {
                _reader.Read();
            }
        }
    }
}
