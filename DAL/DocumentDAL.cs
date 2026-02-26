using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class DocumentDAL : GenericService<Document>
    {
        private static DbWorker _DbWorker;
        public DocumentDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<List<Document>> GetList(string name, int status, int pageIndex, int pageSize)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Name", string.IsNullOrEmpty(name) ? DBNull.Value : (object)name),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@PageIndex", pageIndex),
                    new SqlParameter("@PageSize", pageSize)
                };

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_Documents_GetList, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<Document>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - DocumentDAL: " + ex);
            }
            return new List<Document>();
        }

        public async Task<int> GetCount(string name, int status)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@DocumentName", string.IsNullOrEmpty(name) ? DBNull.Value : (object)name),
                    new SqlParameter("@Status", status == -1 ? DBNull.Value : (object)status)
                };

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_Documents_GetCount, objParam);
                
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetCount - DocumentDAL: " + ex);
            }
            return 0;
        }

        public async Task<Document> GetById(int id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Id", id)
                };

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_Documents_GetById, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var list = dt.ToList<Document>();
                    return list.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - DocumentDAL: " + ex);
            }
            return null;
        }

        public async Task<int> Insert(Document model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@DocumentName", model.DocumentName),
                    new SqlParameter("@Category", model.Category ?? (object)DBNull.Value),
                    new SqlParameter("@Description", model.Description ?? (object)DBNull.Value),
                    new SqlParameter("@FilePath", model.FilePath ?? (object)DBNull.Value),
                    new SqlParameter("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value),
                    new SqlParameter("@CreatedDate", model.CreatedDate),
                    new SqlParameter("@Notes", model.Notes ?? (object)DBNull.Value),
                    new SqlParameter("@Status", model.Status),
                    new SqlParameter("@PhysicalStorageLocation", model.PhysicalStorageLocation ?? (object)DBNull.Value),
                    new SqlParameter("@DigitalStorageLocation", model.DigitalStorageLocation ?? (object)DBNull.Value),
                    new SqlParameter("@ImgLocation", model.ImgLocation ?? (object)DBNull.Value)
                };

               return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_Documents_Insert, objParam);
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - DocumentDAL: " + ex);
            }
            return 0;
        }

        public async Task<int> Update(Document model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@DocumentName", model.DocumentName),
                    new SqlParameter("@Category", model.Category ?? (object)DBNull.Value),
                    new SqlParameter("@Description", model.Description ?? (object)DBNull.Value),
                    new SqlParameter("@FilePath", model.FilePath ?? (object)DBNull.Value),
                    new SqlParameter("@Notes", model.Notes ?? (object)DBNull.Value),
                    new SqlParameter("@Status", model.Status),
                    new SqlParameter("@PhysicalStorageLocation", model.PhysicalStorageLocation ?? (object)DBNull.Value),
                    new SqlParameter("@DigitalStorageLocation", model.DigitalStorageLocation ?? (object)DBNull.Value),
                    new SqlParameter("@ImgLocation", model.ImgLocation ?? (object)DBNull.Value)
                };

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_Documents_Update, objParam);
                
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - DocumentDAL: " + ex);
            }
            return 0;
        }
    }
}
