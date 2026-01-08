

using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class FlashSaleDAL : GenericService<FlashSale>
    {
        private static DbWorker _DbWorker;

        public FlashSaleDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public int CreateFlashSale(FlashSale model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@Status", model.Status);
                objParam[1] = new SqlParameter("@UserCreateId", model.UserCreateId);
                objParam[2] = new SqlParameter("@Name", model.Name);
    

                model.Id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertFlashSale, objParam);
                return model.Id; // DbWorker sẽ tự động trả về giá trị của @Identity
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateFlashSale - FlashSaleDAL: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Cập nhật thông tin Flash Sale.
        /// </summary>
        /// <param name="model">Đối tượng FlashSale chứa thông tin cập nhật.</param>
        /// <returns>ID của Flash Sale đã được cập nhật, hoặc 0 nếu có lỗi.</returns>
        public int UpdateFlashSale(FlashSale model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@Id", model.Id);
                objParam[1] = new SqlParameter("@Status", model.Status);
                objParam[2] = new SqlParameter("@UserUpdateId", model.UserUpdateId);
                objParam[3] = new SqlParameter("@Name", model.Name);
  
                // Gọi ExecuteNonQuery và nhận giá trị trả về (ID đã cập nhật)
                int updatedId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateFlashSale, objParam);
                return updatedId; // DbWorker sẽ tự động trả về giá trị của @Identity
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateFlashSale - FlashSaleDAL: " + ex.Message);
                return 0;
            }
        }
        public async Task<DataTable> GetList(DateTime? fromdate, DateTime? todate, int status, int page_index, int page_size)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[5];
                objParam[0] = new SqlParameter("@PageIndex", page_index);
                objParam[1] = new SqlParameter("@PageSize", page_size);
                objParam[2] = new SqlParameter("@FromDate", fromdate ?? (object)DBNull.Value);
                objParam[3] = new SqlParameter("@ToDate", todate ?? (object)DBNull.Value);
                objParam[4] = new SqlParameter("@FlashSaleStatusFilter", status);
               
                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListFlashSale, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - FlashSaleDAL: " + ex);
            }
            return null;
        }
        public async Task<FlashSale> GetByID(int id)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.FlashSales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByID - FlashSaleDAL: " + ex.ToString());
                return null;
            }
        }
    }

}

