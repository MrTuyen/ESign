using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Permission
{
    public class PermissionGroupBO
    {
        public int ID { get; set; }
        public string PERMISSIONGROUPNAME { get; set; }
        public List<PermissionBO> PERMISSIONLIST { get; set; }
    }
}
