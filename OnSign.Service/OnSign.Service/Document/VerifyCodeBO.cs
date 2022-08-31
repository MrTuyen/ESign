using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Document
{
    public class VerifyCodeBO
    {
        public long ID { get; set; }
        public long ID_REQUEST { get; set; }
        public string UUID { get; set; }
        public string CODE { get; set; }
        public DateTime CREATED_AT_TIME { get; set; }
        public string PHONE_NUMBER { get; set; }
        public string PHONE_NUMBER_CALL_OUT { get; set; }
        public string CREATED_BY_IP { get; set; }
        public string CREATED_BY_EMAIL { get; set; }
        public int CREATED_BY_USER { get; set; }
        public bool IS_ACTIVED { get; set; }
        public DateTime ACTIVED_AT_TIME { get; set; }
        public bool IS_CALLED { get; set; }
        public DateTime CALLED_AT_TIME { get; set; }
        public int TYPE { get; set; }
    }
}
