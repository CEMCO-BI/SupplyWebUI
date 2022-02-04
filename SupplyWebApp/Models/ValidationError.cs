using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    public class ValidationError
    {

        public int LineNumber { get; set; }
        public string ErrorValidateMessage { get; set; }

        public ValidationError(int line, string error)
        {
            this.LineNumber = line;
            this.ErrorValidateMessage = error;
        }
    }
}