using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.TemplateBO
{
    public class DocumentTemplateBookmark
    {
        public long ID { get; set; }
        public long DOCTEMPLATEID { get; set; }
        public string NAME { get; set; }
        public string VALUE { get; set; }
        public long DOCID { get; set; }
        public long PDFID { get; set; }
    }
}
