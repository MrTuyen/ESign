using Microsoft.SqlServer.Server;
using OnSign.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OnSign.BusinessObject.Permission
{
    public class PermissionBO
    {
        public int ID { get; set; }
        public string PERMISSIONNAME { get; set; }
        public int PERMISSIONGROUPID { get; set; }
        public string PERMISSIONGROUPNAME { get; set; }
        public bool ISSELECTED { get; set; }
    }
}
