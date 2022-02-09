using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    public class CRUPricingValidateObj
    {
        public string date_v { get; set; }
        public string Week1_v { get; set; }
        public string Week2_v { get; set; }
        public string Week3_v { get; set; }
        public string Week4_v { get; set; }
        public string Week5_v { get; set; }


        public CRUPricingValidateObj( string v2, string v3, string v4, string v5, string v6, string v7)
        {
            this.date_v = v2;
            this.Week1_v = v3;
            this.Week2_v = v4;
            this.Week3_v = v5;
            this.Week4_v = v6;
            this.Week5_v = v7;
        }
    }
}
