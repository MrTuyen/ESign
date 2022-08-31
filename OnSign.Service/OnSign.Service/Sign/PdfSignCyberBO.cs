namespace OnSign.BusinessObject.Sign
{
    public class PdfSignCyberBO
    {
        public string base64pdf { get; set; }
        public string hashalg { get; set; }
        public int typesignature { get; set; }
        public string signaturename { get; set; }
        public string base64image { get; set; }
        public string textout { get; set; }
        public int pagesign { get; set; }
        public int xpoint { get; set; }
        public int ypoint { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}
