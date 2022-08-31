using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Partners
{
    public class PG_ALERTBO
    {
        public const string ACTION_INSERT = "INSERT";
        public const string ACTION_UPDATE = "UPDATE";
        public const string ACTION_DELETE = "DELETE";

        public const string TABLE_SYSTEM_USER = "system_user";
        public const string TABLE_PM_REQUEST = "pm_request";
        public const string TABLE_PM_DOCUMENT_SIGN = "pm_document_sign";
        public const string TABLE_PM_DOCUMENT_TEMPLATE_PDF = "pm_document_template_pdf";

        public string USERNAME { get; set; }
        public string ACTION { get; set; }
        public string TABLE { get; set; }
        public object DATA { get; set; }
    }
}
