using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("Warehouse", Schema = "dbo")]
    public class Warehouse
    {
        [Key]
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Abbr { get; set; }

    }
}
