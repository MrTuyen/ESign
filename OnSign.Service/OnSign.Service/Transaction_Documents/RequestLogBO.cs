using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Transaction_Documents
{
    public class RequestLogBO
    {
        public long ID { get; set; }
        public long ID_REQUEST { get; set; }
        public string UUID { get; set; }
        public int CREATED_BY_USER { get; set; }
        public DateTime CREATED_AT_TIME { get; set; }
        public string CREATED_BY_IP { get; set; }
        public string ACTION { get; set; }
        public string MESSAGES { get; set; }
        public string TYPE { get; set; }
    }
}
