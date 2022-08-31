using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Logs
{
    public class LogItemBO
    {
        public long ID { get; set; }
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }
        public string ACTIONNAME { get; set; }
        public string SOURCE { get; set; }
        public long IDREQUEST { get; set; }
        public string UUID { get; set; }
        public string OLDCONTENT { get; set; }
        public string NEWCONTENT { get; set; }
    }
}
