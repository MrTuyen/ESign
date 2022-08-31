using OnSign.BusinessObject.Permission;
using SAB.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnSign.DataObject.Permission
{
    public class PermissionDAO : BaseDAO
    {
        public List<PermissionGroupBO> GetPermissionGroup()
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_permission_group_get");
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<PermissionGroupBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public List<PermissionBO> GetPermissionByPermissionGroupID(int id)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_permission_get_by_permission_group_id");
                objIData.AddParameter("p_permission_group_id", id);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<PermissionBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public List<PermissionBO> GetAllPermission(int? permissionGroupID = null)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_permission_getall");
                objIData.AddParameter("p_permission_group_id", permissionGroupID);
                var reader = objIData.ExecStoreToDataReader();
                var list = new List<PermissionBO>();
                ConvertToObject(reader, list);
                reader.Close();
                CommitTransactionIfAny(objIData);
                return list;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public bool AddingPermissionForUser(PermissionUserBO reg)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.pm_permission_user_add");
                objIData.AddParameter("p_createdbyuser", reg.CREATEDBYUSER);
                objIData.AddParameter("p_createdbyip", reg.CREATEDBYIP);
                objIData.AddParameter("p_permissionid", reg.PERMISSIONID);
                objIData.AddParameter("p_email", reg.EMAIL);
                objIData.AddParameter("p_invitationid", reg.INVITATIONID);

                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);

                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                this.DisconnectIData(objIData);
            }
        }

        public bool UpdatePermission(string email)
        {
            IData objIData = this.CreateIData();
            try
            {
                BeginTransactionIfAny(objIData);
                objIData.CreateNewStoredProcedure("ds_masterdata.system_user_permission_update");
                objIData.AddParameter("p_email", email);
                var reader = objIData.ExecNonQuery();
                CommitTransactionIfAny(objIData);
                return true;
            }
            catch (Exception objEx)
            {
                RollBackTransactionIfAny(objIData);
                throw objEx;
            }
            finally
            {
                DisconnectIData(objIData);
            }
        }

    }
}
