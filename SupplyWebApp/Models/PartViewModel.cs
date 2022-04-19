using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("Part", Schema = "dbo")]
    public class Part
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PartId { get; set; }
        public string PartNo { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        
    }
}
