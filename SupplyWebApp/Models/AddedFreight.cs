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
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        [Key]
        public int Id { get; set; }

        [ForeignKey("Location")]
        public int? POLocationId { get; set; }
        public Location Location { get; set; }

        [ForeignKey("Warehouse")]
        public int? POWarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }



        [ForeignKey("Carrier")]
        public int? POCarrierId { get; set; }
        public Carrier Carrier { get; set; }
        
        [ForeignKey("Vendor")]
        public int? VendorId { get; set; }
        public Vendor Vendor { get; set; }


        public double CWT { get; set; }
        public string TruckLoad { get; set; }

        

    }
}
