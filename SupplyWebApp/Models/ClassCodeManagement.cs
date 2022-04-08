using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    [Table("ClassCodeManagement", Schema = "Upload")]
    public class ClassCodeManagement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int Class_CodeID { get; set; }
        public int Product_codeId { get; set; }
        public int LocationId { get; set; }
        public int Active { get; set; }
    }
}
