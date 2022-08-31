using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Document
{
    public class ReportDocumentBO
    {

        /// <summary>
        /// Tổng số lượng luồng ký hoàn thành của công ty
        /// </summary>
        public long COMPANY_REQUEST_FINISH { get; set; }

        /// <summary>
        /// Số lượng luồng ký hoàn thành của thành viên
        /// </summary>
        public long USER_REQUEST_FINISH { get; set; }

        /// <summary>
        /// Tổng số lượng chữ ký đã sử dụng của công ty
        /// </summary>
        public long COMPANY_SESSION_SIGNED { get; set; }

        /// <summary>
        /// Số lượng chữ ký đã sử dụng của thành viên
        /// </summary>
        public long USER_SESSION_SIGNED { get; set; }
    }
}
