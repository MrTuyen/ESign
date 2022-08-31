using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.TemplateBO
{
    public class DocumentTemplatePdf
    {
        public long ID { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public int CREATEDBYUSER { get; set; }
        public string CREATEDBYIP { get; set; }
        public string PATH { get; set; }
        public string NAME { get; set; }
        public string ICON { get; set; }
        public long SIZE { get; set; }
        public int PAGES { get; set; }
        public int COMPLETEPERCENT { get; set; } = 0;
        public long DOCTEMPLATEID { get; set; }
        public bool STATUS { get; set; }
        public int TOTALROW { get; set; }
        public string IDCONTRACT { get; set; }
        public string EMAIL_SUBJECT { get; set; }
        public string EMAIL_CONTENT { get; set; }
        public string REQUEST_UUID { get; set; }
        public Guid TempID { get; set; }
        public List<DocumentTemplateData> DocumentDatas { get; set; }
        public List<DocumentTemplateReceive> Receives { get; set; }
        public DocumentTemplatePdf()
        {
            Receives = new List<DocumentTemplateReceive>();
            DocumentDatas = new List<DocumentTemplateData>();
        }
    }
}
