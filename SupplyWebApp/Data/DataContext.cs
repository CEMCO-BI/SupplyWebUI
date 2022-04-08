using Microsoft.EntityFrameworkCore;
using SupplyWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<SalesForecast> SalesForecast { get; set; }
        public DbSet<PlannedBuy> PlannedBuy { get; set; }
        public DbSet<CRUPricing> CRUPricing { get; set; }
        public DbSet<AddedFreight> AddedFreight { get; set; }
        public DbSet<TransferFreight> TransferFreight { get; set; }
        public DbSet<ClassCodeManagement> ClassCodeManagement { get; set; }
        public DbSet<DisplayMonths> DisplayMonths { get; set; }

    }
}
