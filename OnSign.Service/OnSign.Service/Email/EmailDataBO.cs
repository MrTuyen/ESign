using System.Collections.Generic;
using System.IO;

namespace OnSign.BusinessObject.Email
{
    public class EmailDataBO
    {
        public long ID { get; set; }
        public string EmailType { get; set; }
        public int CreatedByUser { get; set; }
        public string CreatedByEmail { get; set; }
        public string CreatedDateTime { get; set; }
        public string CreatedByIP { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string MailTo { get; set; }
        public string MailName { get; set; }
        public string Messages { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string Subject { get; set; }
        public string CC { get; set; }
        public bool ISCC { get; set; }
        public string BCC { get; set; }
        public string DocumentLinkViewer { get; set; }
        public string Content { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
        public string UUID { get; set; }
        public bool? IsSent { get; set; }
        public bool? IsOpened { get; set; }
        public bool? IsReSend { get; set; }

        public string DocumentLinkLogo { get; set; }
        public string DocumentMessage { get; set; }
        public List<Stream> StreamAttachment { get; set; }
        public List<string> FileName { get; set; }

    }
}
