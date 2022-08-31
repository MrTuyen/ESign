using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.TemplateBO
{
    public class DocumentTemplateReceive
    {
        public long ID { get; set; }
        public string IDCONTRACT { get; set; }
        public string NAMEFROM { get; set; }
        public string EMAILFROM { get; set; }
        public string REQUESTSIGNTYPE { get; set; }
        public string EMAILTO { get; set; }
        public string NAMETO { get; set; }
        public string TAXCODE { get; set; }
        public string ADDRESS { get; set; }
        public string IDNUMBER { get; set; }
        public string PHONENUMBER { get; set; }
        public long PDFID { get; set; }
        public string SUBJECT { get; set; }
        public string CONTENT { get; set; }
        public bool ISCOMPANY { get; set; }
        public bool SENT { get; set; }
        public int? SIGNINDEX { get; set; }
        public string REQUESTUUID { get; set; }
        public Guid TempID { get; set; }
    }
}
