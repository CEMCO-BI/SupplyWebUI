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
        public int TransferFromId { get; set; }
        public int TransferToId { get; set; }
        public string? ProductCode { get; set; }
        public double TransferCost { get; set; }
    }
}
