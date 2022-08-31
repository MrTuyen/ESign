using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Sign
{
    public class PdfSignedBO
    {
        public string error { get; set; }
        public int status { get; set; }
        public string description { get; set; }
        public string obj { get; set; }
    }
}
