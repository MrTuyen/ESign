using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Permission
{
    public class PermissionUserBO
    {
        public long ID { get; set; }
        public int CREATEDBYUSER { get; set; }
        public DateTime CREATEDATTIME { get; set; }
        public string CREATEDBYIP { get; set; }
        public int PERMISSIONID { get; set; }
        public bool PERMISSIONSTATUS { get; set; }
        public int USERID { get; set; }
        public string EMAIL { get; set; }
        public int INVITATIONID { get; set; }
    }
}
