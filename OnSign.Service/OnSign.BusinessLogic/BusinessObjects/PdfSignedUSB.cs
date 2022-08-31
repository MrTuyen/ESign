using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.BusinessObjects
{
    public class PdfSignedUSB
    {
        public int code { get; set; }
        public int index { get; set; }
        public string data { get; set; }
        public string type { get; set; }
        public string error { get; set; }
        public string outputpath { get; set; }
        public string subject { get; set; }
    }
}
