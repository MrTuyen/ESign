using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Sign
{
    public class ReceiverBO
    {
        public long ID { get; set; }
        public string UUID { get; set; }
        public string CREATEDBYIP { get; set; }
        public int CREATEDBYUSER { get; set; }
        public long IDREQUEST { get; set; }
        public string NAME { get; set; }
        public string EMAIL { get; set; }
        public string TAXCODE { get; set; }
        public string ADDRESS { get; set; }
        public string IDNUMBER { get; set; }
        public string PHONENUMBER { get; set; }
        public bool ISCC { get; set; }
        public bool ISCCFINISH { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string REQUESTSIGNTYPE { get; set; }
    }

}
