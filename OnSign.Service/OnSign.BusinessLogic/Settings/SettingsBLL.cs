using OnSign.Common.Helpers;
using OnSign.DataObject.Account;
using OnSign.BusinessObject.Account;
using OnSign.BusinessObject.Forms;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.BusinessLogic.Settings
{
    public class SettingsBLL : BaseBLL
    {
        #region Fields
        protected IData objDataAccess = null;

        #endregion

        #region Properties

        #endregion

        #region Constructor
        public SettingsBLL()
        {
        }

        public SettingsBLL(IData objIData)
        {
            objDataAccess = objIData;
        }
        #endregion

        #region Methods
        
        #endregion
    }

}
