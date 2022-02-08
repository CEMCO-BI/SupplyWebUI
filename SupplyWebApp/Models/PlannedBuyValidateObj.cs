using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{


    public class PlannedBuyValidateObj
    {




        public string Year_v { get; set; }
        public string Month_v { get; set; }
        public string Location_v { get; set; }
        public string Amount_v { get; set; }

        public PlannedBuyValidateObj(string v1, string v2, string v3, string v4)
        {
            this.Year_v = v1;
            this.Month_v = v2;
            this.Location_v = v3;
            this.Amount_v = v4;
        }
    }
}
