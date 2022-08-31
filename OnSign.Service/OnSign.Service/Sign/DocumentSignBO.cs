using System;

namespace OnSign.BusinessObject.Sign
{
    public class DocumentSignBO
    {
        public long ID { get; set; }
        public long IDDOC { get; set; }
        public long IDREQUEST { get; set; }
        public string DOCPATH { get; set; }
        public string UUID { get; set; }
        public int CREATEDBYUSER { get; set; }
        public string CREATEDBYIP { get; set; }
        public string TYPESIGN { get; set; }
        //Tọa độ vị trí chữ ký
        public float XPOINT { get; set; }
        public float YPOINT { get; set; }
        //Kích thước của chữ ký
        public float SIGNATUREHEIGHT { get; set; }
        public float SIGNATUREWIDTH { get; set; }
        //Đường dẫn của chứ ký
        public string SIGNATUREPATH { get; set; }
        //Chữ ký dạng base64 hoặc url
        public string SIGNATUREIMAGE { get; set; }
        public string SIGNATURETEXT { get; set; }
        public int PAGESIGN { get; set; }
        public int PAGE { get; set; }

        //Kích thước của hình ảnh hiện thị file pdf
        public float WIDTHIMAGE { get; set; }
        public float HEIGHTIMAGE { get; set; }

        //Điểm bắt đầu và kết thúc của 1 page
        public float STARTY { get; set; }
        public float ENDY { get; set; }

        //Margin bottom khoảng cách giữa 2 file ảnh
        public float MARGINBOTTOM { get; set; }

        //Kích thước page file pdf
        public int PDFWIDTH { get; set; }
        public int PDFHEIGHT { get; set; }

        public bool ISDELETED { get; set; }
        public bool ISSIGNED { get; set; }
        public bool ISINITIAL { get; set; }
        public string EMAILASSIGNMENT { get; set; }
        public string EMAILASSIGNMENTNAME { get; set; }
        public int SIGNINDEX { get; set; }
        public string SIGNEDBYIP { get; set; }
        public bool ISDECLINED { get; set; }
        public string DECLINEBYIP { get; set; }
        public DateTime SIGNEDATTIME { get; set; }
        public string CERINFO { get; set; }
        public bool ISUSEUSBTOKEN { get; set; }
        public CERTINFOBO CERINFOBO { get; set; }
        public string TAXCODE { get; set; }
        public string ADDRESS { get; set; }
        public string IDNUMBER { get; set; }
        public string PHONENUMBER { get; set; }
        public string REQUESTSIGNTYPE { get; set; }
    }
}
