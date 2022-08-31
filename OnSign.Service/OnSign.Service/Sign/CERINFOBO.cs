using Org.BouncyCastle.X509;
using System;

namespace OnSign.BusinessObject.Sign
{
    public class CERTINFOBO
    {
        //Status
        public bool SIGNED { get; set; }
        //Địa chỉ email
        public string STATUS { get; set; }
        //ID rq
        public DateTime? CERSTARTDATE { get; set; }
        //Nhận bản sao hay k (CC)
        public DateTime? CERENDDATE { get; set; }
        //Người ký thứ mấy
        public string SUPPLIER { get; set; }
        public string SUPPLIERNAME { get; set; }
        public string TAXCODE { get; set; }
        public string COMPANY { get; set; }
        public string SERIAL { get; set; }
        public string CERINFO { get; set; }
        public bool ISUSEUSBTOKEN { get; set; }
        public X509Certificate certificate { get; set; }
    }
}
