using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Partners
{
    public class CyberLotusBO
    {
        public string APIID { get; set; }
        public string APIURL { get; set; }
        public string SECRET { get; set; }
        public string TAXCODE { get; set; }
        public bool ISCOMPANY { get; set; }
    }
}
