using System;

namespace OnSign.BusinessObject.Account
{
    public class LTemplateBO
    {
        public long ID { get; set; }
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public int ACTIONID { get; set; }
        public string ACTIONNAME { get; set; }
        public string OLDCONTENT { get; set; }
        public string NEWCONTENT { get; set; }
        public long IDREQUEST { get; set; }
        public string CREATEDBYIP { get; set; }
        public string SOURCE { get; set; }
        public int TOTALROW { get; set; }
    }
}

