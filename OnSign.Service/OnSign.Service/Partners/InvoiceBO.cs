using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Partners
{
    public class InvoiceBO
    {
        public int ID { get; set; }
        public string FORMCODE { get; set; }
        public string SYMBOLCODE { get; set; }
        public DateTime CREATEDTIME { get; set; }
        public string CUSNAME { get; set; }
        public int NUMBER { get; set; }
        public string LINKVIEW { get; set; }
        public string CUSTAXCODE { get; set; }
        public double TOTALPAYMENT { get; set; }
        public bool ISSELECTED { get; set; }

    }

    public class InvoiceResultBO
    {
        public bool rs { get; set; }
        public string msg { get; set; }
        public List<InvoiceBO> result { get; set; }
        public int TOTALPAGES { get; set; }
        public int TOTALROW { get; set; }
        public int CURRENTPAGE { get; set; }
    }
}
