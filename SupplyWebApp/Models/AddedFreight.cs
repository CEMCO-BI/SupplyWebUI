using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("AddedFreight", Schema = "Upload")]
    public class AddedFreight
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [ForeignKey("Location")]
        public int POLocationId { get; set; }
        public Location Location { get; set; }
        public int? POWarehouseId { get; set; }
        public int? POCarrierId { get; set; }
        public int? VendorId { get; set; }
        public string CWT { get; set; }
        public string TruckLoad { get; set; }

        

    }
}
