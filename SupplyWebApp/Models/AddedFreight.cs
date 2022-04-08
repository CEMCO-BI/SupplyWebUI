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

        public int PO_LocationId { get; set; }
        public int PO_WarehouseId { get; set; }
        public int PO_CarrierId { get; set; }
        public int VendorId { get; set; }
        public string CWT { get; set; }
        public string TruckLoad { get; set; }

    }
}
