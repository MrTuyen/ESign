using System;

namespace OnSign.BusinessObject.Notifications
{
    public class NotificationItemBO
    {
        public long ID { get; set; }
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }
        public string ICON { get; set; }
        public String TITLE { get; set; }
        public string MESSAGES { get; set; }
        public bool ISSEEN { get; set; }
        public bool ISDELETED { get; set; }
    }
}
