using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("ClassCode", Schema = "dbo")]
    public class ClassCode
    {
        [Key]
        public int ClassCodeId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

    }
}
