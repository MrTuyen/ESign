using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.Forms;
using OnSign.BusinessObject.Partners;
using OnSign.BusinessObject.Sign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.BusinessObjects
{
    public class ObjectResult
    {
        public bool rs { get; set; }
        public string msg { get; set; }
        public RequestSignBO dataResponse { get; set; }
        public List<RequestSignBO> RequestSignBOs { get; set; }
        public bool iscc { get; set; }
        public string email { get; set; }
        public int signIndex { get; set; }
        public int type { get; set; }
        public AccountBO objUser { get; set; }
        public bool isReload { get; set; }
        public long idrequest { get; set; }
        public bool signUSB { get; set; }
        public List<PdfSignUSB> pdfSignUSBs { get; set; }
        public string accessToken { get; set; }
        public string view { get; set; }
        public List<InvoiceBO> InvoiceBOs { get; set; }
        public long TOTALROW { get; set; }
        public long? TOTALPAGES { get; set; }
        public int? CURRENTPAGE { get; set; }

    }
}
