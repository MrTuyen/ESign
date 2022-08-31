using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Company
{
    /// <summary>
    /// phungpd 11/12/2020
    /// Chia gói sử dụng
    /// </summary>
    public class PackageBO : BaseBO
    {
        /// <summary>
        /// ID của package
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// Tên package
        /// </summary>
        public string PACKAGE_NAME { get; set; }

        /// <summary>
        /// Được khởi tạo bởi ai (thường sẽ là user của CS)
        /// </summary>
        public int CREATED_BY_USER { get; set; }

        /// <summary>
        /// Được khởi tạo bởi ai tên
        /// </summary>
        public int CREATED_BY_USER_NAME { get; set; }

        /// <summary>
        /// Được khởi tạo thời gian nào
        /// </summary>
        public DateTime CREATED_AT_TIME { get; set; }

        /// <summary>
        /// Được khởi tạo bởi IP nào
        /// </summary>
        public string CREATED_BY_IP { get; set; }

        /// <summary>
        /// User nào sẽ là người sử dụng
        /// </summary>
        public int USER_ID { get; set; }

        /// <summary>
        /// Tên người sử dụng
        /// </summary>
        public int USER_NAME { get; set; }

        /// <summary>
        /// Mã số thuế của user (USER_ID)
        /// </summary>
        public string TAX_CODE { get; set; }

        /// <summary>
        /// Thời gian bắt đầu sử dụng
        /// </summary>
        public DateTime USING_FROM_DATE { get; set; }

        /// <summary>
        /// Thời gian kết thúc
        /// </summary>
        public DateTime USING_TO_DATE { get; set; }

        /// <summary>
        /// Gia hạn sử dụng
        /// </summary>
        public DateTime EXTEND_USING_TO_DATE { get; set; }

        /// <summary>
        /// Tổng Số lượng hợp đồng 
        /// </summary>
        public int TOTAL_FLOW_NUMBER { get; set; }

        /// <summary>
        /// Số lượng hợp đồng sử dụng hiện tại
        /// </summary>
        public int CURRENT_FLOW_NUMBER { get; set; }

        /// <summary>
        /// Tổng Số phiên ký HSM
        /// </summary>
        public int TOTAL_HSM_NUMBER { get; set; }

        /// <summary>
        /// Số lượng phiên ký HSM hiện tại
        /// </summary>
        public int CURRENT_HSM_NUMBER { get; set; }

        /// <summary>
        /// Cờ kích hoạt hay chưa
        /// </summary>
        public bool IS_ACTIVED { get; set; }
    }
}
