using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using Utilities;
using Utilities.Contants;

namespace DAL.Identifier
{
    public class IdentifierDAL : GenericService<Order>
    {
        private static DbWorker _DbWorker;


        public IdentifierDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public int countServiceUse(int service_type, int? tenant_id = null)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@ServiceType", service_type);
                objParam[1] = new SqlParameter("@TenantId", (tenant_id == null ? DBNull.Value : (int)tenant_id));

                DataTable tb = new DataTable();
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_countServiceUse, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("service_type - IdentifierDAL: " + ex.ToString());
                return -1;
            }
        }
        public long CountIdentity(int code_type)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ServiceType", code_type);

                DataTable tb = new DataTable();
                return _DbWorker.ExecuteNonQuery("Sp_CountServiceUse", objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountIdentity - IdentifierDAL: " + ex.ToString());
                return -1;
            }
        }

        public int countClientTypeUse(int client_type, int? tenant_id = null)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@ClientType", client_type);
                objParam[1] = new SqlParameter("@TenantId", (tenant_id==null? DBNull.Value:(int)tenant_id));

                DataTable tb = new DataTable();
                return _DbWorker.ExecuteNonQuery("Sp_CountClientByType", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("countClientTypeUse - IdentifierDAL: " + ex.ToString());
                return -1;
            }
        }  
        public int CountRequestByDay(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@FromDate", FromDate);
                objParam[1] = new SqlParameter("@ToDate", ToDate);

                DataTable tb = new DataTable();
                return _DbWorker.ExecuteNonQuery("Sp_CountRequestByDay", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("countClientTypeUse - IdentifierDAL: " + ex.ToString());
                return -1;
            }
        }
    }
}
