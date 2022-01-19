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

namespace SupplyWebApp.Services
{
    public abstract class ImportService
    {
        public DataContext DataContext;
        protected IHostEnvironment _hostingEnvironment;

        public virtual void Import(IFormFile file)
        {

        }
    }
}
