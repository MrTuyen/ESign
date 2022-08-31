using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject
{
    public class BaseBO
    {
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }

        public bool? ISONLYMESIGN { get; set; }
        public bool? ISDELETED { get; set; }
        public string DELETEDBYUSER { get; set; }
    }
}
