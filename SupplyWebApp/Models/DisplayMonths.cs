using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("DisplayMonths", Schema = "Upload")]
    public class DisplayMonths
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Month { get; set; }
        public int Year { get; set; }
        public int Active { get; set; }
    }
}
