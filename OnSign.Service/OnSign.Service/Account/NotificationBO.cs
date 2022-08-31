using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Account
{
    public class NTemplateBO
    {
        public long ID { get; set; }
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }
        public string  ICON { get; set; }
        public String TITLE { get; set; }
        public string MESSAGES { get; set; }
        public bool ISSEEN { get; set; }
        public bool ISDELETED { get; set; }
        public int TOTALROW { get; set; }
    }
}
