using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyWebApp.Models
{
    public class ValidationError
    {

        public int LineNumber { get; set; }
        public string ErrorMessage { get; set; }

        public object RowData { get; set; }

        public ValidationError(int line, string error, object rowData)
        {
            this.LineNumber = line;
            this.ErrorMessage = error;
            this.RowData = rowData;
        }
    }
}