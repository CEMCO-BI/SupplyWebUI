using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    public class ImportResult
    {
        public bool Successful { get; set; } = true;
        public string Message { get; set; }

        public List<ValidationError> ErrorList = new List<ValidationError>();
    }
}
