using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("CRUPricing", Schema = "Upload")]

    public class CRUPricing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Week { get; set; }
        public double Amount { get; set; }
    }
}
