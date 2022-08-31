using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Forms
{
    public class CompanyFormBO
    {
        public string TAXCODE { get; set; }
        public string COMPANY { get; set; }
        public string ADDRESS { get; set; }
        public string PHONE { get; set; }
        public string FULLNAME { get; set; }
        public bool ISACTIVED { get; set; }
        public int CREATEDBYUSER { get; set; }
        public string CREATEDBYIP { get; set; }
    }
}
