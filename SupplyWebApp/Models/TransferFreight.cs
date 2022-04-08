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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Transfer_from_Id { get; set; }
        public int Transfer_to_Id { get; set; }
        public string? Product_Code { get; set; }
        public string Transfer_Cost { get; set; }
    }
}
