using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Company
{
    public class CTemplateBO
    {
        public int ID { get; set; }
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }
        public string TAXCODE { get; set; }
        public string COMPANY { get; set; }
        public string ADDRESS { get; set; }
        public string PHONE { get; set; }
        public string FULLNAME { get; set; }
        public bool ISACTIVED { get; set; }
        public int TOTALROW { get; set; }
        public string CREATEDFULLNAME { get; set;}
        public string APIID { get; set; }
        public string APIURL { get; set; }
        public string USERNAME { get; set; }
        public string EMAIL { get; set; }
        public string POSITION { get; set; }
        public string PASSWORD { get; set; }
        public string SECRET { get; set; }
        public string SIGNATUREIMAGE { get; set; }
        public bool ISADMIN { get; set; }
        public bool ISUSEHSM { get; set; }

    }

    public class CompanyBO
    {
        public int ID { get; set; }
        public string TAXCODE { get; set; }
        public string COMPANY { get; set; }
        public string ADDRESS { get; set; }
        public string PHONE { get; set; }
        public string FULLNAME { get; set; }
        public bool ISACTIVED { get; set; }
        public int TOTALROW { get; set; }
    }
}
