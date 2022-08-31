using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessObject.Output
{
    public class StarBuildParams
    {
        /// <summary>
        /// Status
        /// </summary>
        public string s { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string e { get; set; }

        /// <summary>
        /// ID Request
        /// </summary>
        public string i { get; set; }

        /// <summary>
        /// IS CC
        /// </summary>
        public string c { get; set; }

        /// <summary>
        /// Sign Index
        /// </summary>
        public string p { get; set; }
        
        /// <summary>
        /// id của email (trong bảng pm_email)
        /// </summary>
        public string i_e { get; set; }
    }

    public class InvitationBuildParams
    {
        //Status
        public string i { get; set; }
        //Địa chỉ email
        public string u { get; set; }
        //ID rq
        public string n { get; set; }
        //Nhận bản sao hay k (CC)
        public string e { get; set; }
        //Người ký thứ mấy
        public string m { get; set; }
    }
}
