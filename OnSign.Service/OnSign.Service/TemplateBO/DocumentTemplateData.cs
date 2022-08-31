using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.TemplateBO
{
    public class DocumentTemplateData
    {
        public long ID { get; set; }
        public long DOCID { get; set; }
        public string NAME { get; set; }
        public string VALUE { get; set; }
        public long PDFID { get; set; }
        public string UUROWID { get; set; }
    }
}
