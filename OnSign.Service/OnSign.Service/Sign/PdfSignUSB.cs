using System.Collections.Generic;

namespace OnSign.BusinessObject.Sign
{
    /// <summary>
    /// 1 tài liệu chứ nhiều chữ ký
    /// </summary>
    public class PdfSignUSB
    {
        public long id { get; set; }
        public string type { get; set; }
        public string path { get; set; }
        public string input { get; set; }
        public int index { get; set; }
        public string output { get; set; }
        public List<PdfListSignUSB> pdfListSign { get; set; }
    }
}
