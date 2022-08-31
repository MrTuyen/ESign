using System;

namespace OnSign.BusinessObject.Forms
{
    public class FormSearch
    {
        public long ID { get; set; }
        public int CREATED_BY_USER { get; set; }
        public string TAX_CODE { get; set; }
        public string KEYWORDS { get; set; } = "";
        public string EMAIL { get; set; }
        public string EMAILTO { get; set; }
        public string STATUS { get; set; }
        public string CATEGORY { get; set; }
        public bool ISDELETED { get; set; }
        public string TIME { get; set; }
        public string STRFROMDATE { get; set; }
        public string STRTODATE { get; set; }
        public DateTime? FROMDATE { get; set; }
        public DateTime? TODATE { get; set; }
        public int? CURRENTPAGE { get; set; }
        public int? ITEMPERPAGE { get; set; }
        public int? OFFSET { get; set; }
        public string FILTERSTATUS { get; set; }
        public string FILTERCATEGORY { get; set; }
        public string UUID { get; set; }
        public long? DOCID { get; set; }

    }
}