using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("Vendor", Schema = "dbo")]
    public class Vendor
    {
        [Key]
        public int VendorId { get; set; }
        public string CheckName { get; set; }
        public int CompanyId { get; set; }

    }
}
