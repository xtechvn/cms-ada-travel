using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Entities.ViewModels.SupplierConfig;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    // DAL/TicketDAL.cs
    public class TicketDAL : GenericService<Ticket>
    {
        private readonly DbWorker _db;

        public TicketDAL(string connection) : base(connection)
        {
            _connection = connection;
            _db = new DbWorker(connection);
        }

        public DataTable GetListBySupplier(
       int supplierId, int pageIndex, int pageSize,
       string search, int? status,
       int? playZoneId, int? categoryId, int? ticketTypeId, DateTime? expiredDate,
       out int totalRecords)
        {
            try
            {
                var totalParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                var param = new[]
                {
            new SqlParameter("@SupplierId", supplierId),
            new SqlParameter("@PageIndex", pageIndex),
            new SqlParameter("@PageSize", pageSize),
            new SqlParameter("@Search", (object?)search ?? DBNull.Value),
            new SqlParameter("@Status", (object?)status ?? DBNull.Value),
            new SqlParameter("@PlayZoneId", (object?)playZoneId ?? DBNull.Value),
            new SqlParameter("@CategoryId", (object?)categoryId ?? DBNull.Value),
            new SqlParameter("@TicketTypeId", (object?)ticketTypeId ?? DBNull.Value),
            new SqlParameter("@ExpiredDate", (object?)expiredDate ?? DBNull.Value),
            
            totalParam
        };

                var dt = _db.GetDataTable("SP_TicketGetListBySupplier", param);
                totalRecords = (totalParam.Value == DBNull.Value) ? 0 : Convert.ToInt32(totalParam.Value);
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketDAL.GetListBySupplier: " + ex);
                totalRecords = 0;
                return null;
            }
        }

        public DataTable GetDetail(int id)
        {
            try
            {
                SqlParameter[] prms =
                {
                    new SqlParameter("@TicketId", id)
                };
                return _db.GetDataTable("SP_TicketGetDetail", prms);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - TicketDAL: " + ex);
                return null;
            }
        }
        // ======================================
        // 🔹 Lấy vé theo mã code
        // ======================================
        public DataTable GetByCode(string code)
        {
            try
            {
                SqlParameter[] prms =
                {
            new SqlParameter("@TicketCode", code)
        };

                string query = "SELECT * FROM Ticket WHERE TicketCode = @TicketCode";
                return _db.GetDataTableByQuery(query, prms); // ✅ dùng hàm mới
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByCode - TicketDAL: " + ex);
                return null;
            }
        }


        // ======================================
        // 🔹 Cập nhật trạng thái vé (Status + SoldDuration)
        // ======================================
        public bool UpdateStatusByCode(string code, int status)
        {
            try
            {
                string query = @"
            UPDATE Ticket
            SET 
                Status = @Status,
                SoldDuration = GETDATE(),   -- 👈 Thêm dòng này để ghi thời gian sử dụng
                UpdatedDate = GETDATE()
            WHERE 
                TicketCode = @TicketCode
                AND Status = 1
                AND (ExpiredDate IS NULL OR ExpiredDate >= GETDATE());";

                SqlParameter[] prms =
                {
            new SqlParameter("@TicketCode", code),
            new SqlParameter("@Status", status)
        };

                int rows = _db.ExecuteNonQueryByQuery(query, prms);
                return rows > 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateStatusByCode - TicketDAL: " + ex);
                return false;
            }
        }




        public DataTable InsertTicket(TicketListItemViewModel model)
        {
            try
            {
                SqlParameter[] prms =
                {
                    new SqlParameter("@TicketCode", model.TicketCode ?? (object)DBNull.Value),
                    new SqlParameter("@SupplierId", model.SupplierId ?? (object)DBNull.Value),
                    new SqlParameter("@SupplierName", model.SupplierName ?? (object)DBNull.Value),
                    new SqlParameter("@Category", model.Category ?? (object)DBNull.Value),
                    new SqlParameter("@TicketType", model.TicketType ?? (object)DBNull.Value),
                    new SqlParameter("@PlayZone", model.PlayZone ?? (object)DBNull.Value),
                    new SqlParameter("@Status", model.Status ?? (object)DBNull.Value),
                    new SqlParameter("@ImportDate", model.ImportDate ?? (object)DBNull.Value),
                    new SqlParameter("@ExpiredDate", model.ExpiredDate ?? (object)DBNull.Value),
                    // ✅ Bổ sung thêm 3 trường mới
                    new SqlParameter("@ProductId", model.ProductId ?? (object)DBNull.Value),
                    new SqlParameter("@ImportPrice", model.ImportPrice ?? (object)DBNull.Value),
                    new SqlParameter("@TargetAudience", model.TargetAudience ?? (object)DBNull.Value),

                    new SqlParameter("@QRCode", model.QRCode ?? (object)DBNull.Value)
                };

                return _db.GetDataTable("SP_TicketInsert", prms);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertTicket - TicketDAL: " + ex);
                return null;
            }
        }

        public bool UpdateTicket(TicketListItemViewModel model)
        {
            try
            {
                SqlParameter[] prms =
                {
                    new SqlParameter("@TicketId", model.TicketId),
                    new SqlParameter("@TicketCode", model.TicketCode ?? (object)DBNull.Value),
                    new SqlParameter("@SupplierId", model.SupplierId ?? (object)DBNull.Value),
                    new SqlParameter("@SupplierName", model.SupplierName ?? (object)DBNull.Value),
                    new SqlParameter("@Category", model.Category ?? (object)DBNull.Value),
                    new SqlParameter("@TicketType", model.TicketType ?? (object)DBNull.Value),
                    new SqlParameter("@PlayZone", model.PlayZone ?? (object)DBNull.Value),
                    new SqlParameter("@Status", model.Status ?? (object)DBNull.Value),
                    new SqlParameter("@ImportDate", model.ImportDate ?? (object)DBNull.Value),
                    new SqlParameter("@ExpiredDate", model.ExpiredDate ?? (object)DBNull.Value),
                    new SqlParameter("@ProductId", model.ProductId ?? (object)DBNull.Value),
                    new SqlParameter("@ImportPrice", model.ImportPrice ?? (object)DBNull.Value),
                    new SqlParameter("@TargetAudience", model.TargetAudience ?? (object)DBNull.Value),

                    new SqlParameter("@QRCode", model.QRCode ?? (object)DBNull.Value)
                };

                var dt = _db.GetDataTable("SP_TicketUpdate", prms);
                return dt != null && dt.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateTicket - TicketDAL: " + ex);
                return false;
            }
        }
    }

}
