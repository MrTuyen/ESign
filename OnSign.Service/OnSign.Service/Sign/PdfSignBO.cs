using iTextSharp.text;
using OnSign.BusinessObject.Account;
using Org.BouncyCastle.X509;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Sign
{
    public class PdfSignBO
    {
        public string apisign { get; set; }
        public string base64pdf { get; set; }
        public string hashalg { get; set; }
        public int typesignature { get; set; }
        public string signaturename { get; set; }
        public string base64image { get; set; }
        public string textout { get; set; }
        public int pagesign { get; set; }
        public float xpoint { get; set; }
        public float ypoint { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public float pdfwidth { get; set; }
        public float pdfheight { get; set; }
        public Rectangle pdfSize { get; set; }
        public AccountBO objUser { get; set; }
        public string pathOutputPdf { get; set; }
        public string pathOutputPdf_Temp { get; set; }
        public X509Certificate certificate { get; set; }
    }
}
