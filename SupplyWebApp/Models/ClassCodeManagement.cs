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
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [ForeignKey("ClassCode")]
        public int ClassCodeID { get; set; }
        public ClassCode ClassCode { get; set; }

        [ForeignKey("Part")]

        public int ProductCodeId { get; set; }
        public Part Part { get; set; }

        [ForeignKey("Location")]
        public int LocationId { get; set; }
        public Location Location { get; set; }

        public int Active { get; set; }
    }
}
