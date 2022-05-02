using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("TransferFreight", Schema = "Upload")]
    public class TransferFreight
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey(nameof(LocationFrom)), Column(Order = 0)]
        public int TransferFromId { get; set; }
        public Location LocationFrom { get; set; }


        [ForeignKey(nameof(LocationTo)), Column(Order = 1)]
        public int TransferToId { get; set; }
        public Location LocationTo { get; set; }

        [ForeignKey("Part")]

        public int ProductCodeId { get; set; }
        public Part Part { get; set; }
        public double TransferCost { get; set; }
    }
}
