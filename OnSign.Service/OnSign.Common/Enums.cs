using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.Common
{
    public enum HEADERSTATUS
    {
        SUCCESS = 200,
        UNAUTHORIZED = 401,
        NOTFOUND = 404
    }

    public enum HASHALG
    {
        SHA1,
        SHA256,
        SHA512
    }

    public enum TYPESIGNATURE
    {
        NODISPLAY = 0,
        IMAGE = 1,
        TEXT = 2,
        TEXTANDIMAGE = 3
    }

    public class RequestStatus
    {
        public const string CHO_KY = "Chờ ký";
        public const string DA_KY = "Đã ký";
        public const string NHAP = "Nháp";
        public const string DA_XOA = "Đã xóa";
        public const string TU_CHOI = "Từ chối";
        public const string HOAN_THANH = "Hoàn thành";
        public const string DA_GUI = "Đã gửi";
        public const string HDDT = "Hóa đơn điện tử";
        public const string ISCC = "Tôi nhận bản sao";
        public const string CANCEL = "Hủy bỏ";

    }

    public static class DocumentSentItem
    {
        public static string Logo { get; set; }
        public static string Content { get; set; }
    }

    public static class DocumentLogo
    {
        public static string GENERIC { get { return "https://s.onfinance.asia/Images/sign_GENERIC.png"; } }
        public static string REQUEST { get { return "https://s.onfinance.asia/Images/sign_REQUEST.png"; } }
        public static string COMPLETE { get { return "https://s.onfinance.asia/Images/sign_complete.png"; } }
        public static string DECLINE { get { return "https://s.onfinance.asia/Images/sign_decline.png"; } }
    }

}
