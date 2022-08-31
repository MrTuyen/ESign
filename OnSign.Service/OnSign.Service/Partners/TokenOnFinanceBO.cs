using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Partners
{
    public class TokenOnFinanceBO
    {
        public string Token { get; set; }
    }
    public class FormRequestInvoiceBO
    {
        public string COMTAXCODE { get; set; }
        public int INVOICETYPE { get; set; }
        public int INVOICESTATUS { get; set; }
        public int PAYMENTSTATUS { get; set; }
        public string FORMCODE { get; set; }
        public string SYMBOLCODE { get; set; }
        public string CUSTOMER { get; set; }
        public string PHONENUMBER { get; set; }
        public string TAXCODE { get; set; }
        public string STRFROMDATE { get; set; }
        public string STRTODATE { get; set; }
        public string TIME { get; set; }
        public DateTime FROMDATE { get; set; }
        public DateTime TODATE { get; set; }
        public int CURRENTPAGE { get; set; }
        public int ITEMPERPAGE { get; set; }
        public int OFFSET { get; set; }
        public int REPORTYPE { get; set; }
        public string KEYWORD { get; set; }
    }
}
