using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    public class CRUPricing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Id { get; set; }

        public DateTime Date { get; set; }
        public float Week1 { get; set; }
        public float Week2 { get; set; }
        public float Week3 { get; set; }
        public float Week4 { get; set; }
        public float Week5 { get; set; }
    }
}
