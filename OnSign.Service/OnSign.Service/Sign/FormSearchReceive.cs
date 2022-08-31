using System;

namespace OnSign.BusinessObject.Sign
{
    public class FormSearchReceive
    {
        public long IDREQUEST { get; set; }
        public string STATUS { get; set; }
        public bool ISDELETED { get; set; }
        public string TIME { get; set; }
        public string STRFROMDATE { get; set; }
        public string STRTODATE { get; set; }
        public DateTime? FROMDATE { get; set; }
        public DateTime? TODATE { get; set; }
        public int? CURRENTPAGE { get; set; }
        public int? ITEMPERPAGE { get; set; }
        public int? OFFSET { get; set; }

    }

}
