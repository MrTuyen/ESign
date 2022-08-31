using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Document
{
    public class LinkViewerBO
    {
        public long id { get; set; }
        public bool iscc { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public int signIndex { get; set; }
        public long idEmail { get; set; }
    }
}
