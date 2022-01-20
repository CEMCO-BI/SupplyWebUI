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

namespace SupplyWebApp.Services
{
    public abstract class ImportService
    {
        public DataContext DataContext;
        protected IHostEnvironment _hostingEnvironment;
        protected IExcelDataReader _reader;
        protected int _dataStartRow = 2;

        public virtual void Import(IFormFile file)
        {

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
