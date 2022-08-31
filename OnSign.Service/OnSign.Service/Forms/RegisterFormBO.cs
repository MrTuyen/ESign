using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Forms
{
    public class RegisterFormBO
    {
        public string USERNAME { get; set; }
        public string FULLNAME { get; set; }
        public string EMAIL { get; set; }
        public string PHONE { get; set; }
        public string PASSWORD { get; set; }
        public string COMFIRMPASSWORD { get; set; }
        public string TAXCODE { get; set; }
        public int CREATEDBYUSER { get; set; }
        public string CREATEDBYIP { get; set; }
    }
}
