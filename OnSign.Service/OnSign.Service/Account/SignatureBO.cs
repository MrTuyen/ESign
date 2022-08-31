using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Account
{
   public class SignatureBO
    {
        public long ID { get; set; }
        public string PATH { get; set; }
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }
        public bool ISDELETED { get; set; }
        public int DELETEDBYUSER { get; set; }
        public DateTime DELETEDATTIME { get; set; }
        public string UUID { get; set; }
    }
}
