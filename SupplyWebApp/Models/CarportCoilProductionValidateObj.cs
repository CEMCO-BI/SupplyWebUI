using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    public class CarportCoilProductionValidateObj
    {
        public string Year_v { get; set; }
        public string Month_v { get; set; }
        public string ClassCode_v { get; set; }
        public string Amount_v { get; set; }

        public CarportCoilProductionValidateObj(string v1, string v2, string v3, string v4)
        {
            this.Year_v = v1;
            this.Month_v = v2;
            this.ClassCode_v = v3;
            this.Amount_v = v4;
        }
    }
}
