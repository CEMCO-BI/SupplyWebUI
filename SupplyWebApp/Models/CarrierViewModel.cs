using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("Carrier", Schema = "dbo")]
    public class Carrier
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CarrierId { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        //public ICollection<AddedFreight> AddedFreight { get; set; }

    }
}
