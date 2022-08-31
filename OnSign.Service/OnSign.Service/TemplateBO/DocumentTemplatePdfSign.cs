using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.TemplateBO
{
    public class DocumentTemplatePdfSign
    {
        public long ID { get; set; }
        public long PDFID { get; set; }
        public string TYPESIGN { get; set; }
        public int PAGESIGN { get; set; }
        public float XPOINT { get; set; }
        public float YPOINT { get; set; }
        public float SIGNATUREWIDTH { get; set; }
        public float SIGNATUREHEIGHT { get; set; }
        public int PDFHEIGHT { get; set; }
        public int PDFWIDTH { get; set; }
        public int SIGNINDEX { get; set; }
        public bool ISINITIAL { get; set; }
    }
}
