using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("SalesForecast", Schema = "Upload")]

    public class SalesForecast
    {



        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Location { get; set; }
        public double Amount { get; set; }

    }
}
