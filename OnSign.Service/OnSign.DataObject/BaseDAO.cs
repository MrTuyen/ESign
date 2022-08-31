using OnSign.Common.Helpers;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.DataObject
{
    public class BaseDAO
    {
        #region Fields

        protected IData objDataAccess = null;

        #endregion

        #region Properties

        public IData DataAccess
        {
            get { return objDataAccess; }
            set { objDataAccess = value; }
        }

        #endregion

        #region Constructor

        public BaseDAO()
        { }

        public BaseDAO(IData objIData)
        {
            objDataAccess = objIData;
        }

        #endregion

        #region Methods

        public IData CreateIData()
        {
            IData objIData;
            if (objDataAccess == null)
            {
                objIData = Data.CreateData(ConfigHelper.Instance.GetConnectionStringDS(), false);
            }
            else
                objIData = objDataAccess;
            return objIData;
        }

        public void DisconnectIData(IData objIData)
        {
            if (objDataAccess == null)
                objIData.Disconnect();
        }

        public void BeginTransactionIfAny(IData objIData)
        {
            if (objDataAccess == null)
                objIData.BeginTransaction();
        }

        public void CommitTransactionIfAny(IData objIData)
        {
            if (objDataAccess == null)
                objIData.CommitTransaction();
        }

        public void RollBackTransactionIfAny(IData objIData)
        {
            if (objDataAccess == null)
                objIData.RollBackTransaction();
        }

        public void ConvertToObject(IDataReader reader, dynamic lstObj)
        {
            MethodHelper objMethodHelper = new MethodHelper();
            objMethodHelper.ConvertToObject(reader, lstObj);
        }
        #endregion
    }

}
