using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.Apartment;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class ApartmentOrderDAL : GenericService<Hotel>
    {
        private DbWorker _DbWorker;
        public ApartmentOrderDAL(string connection) : base(connection)
        {
            _connection = connection;
            _DbWorker = new DbWorker(connection);
        }
        public int InsertPayment(HotelShareHolderPayment model)
        {
            var param = new SqlParameter[]
            {
        new("@ShareHolderId", model.ShareHolderId),
        new("@HotelId", model.HotelId),
        new("@Amount", model.Amount),
        new("@PayDate", model.PayDate ?? (object)DBNull.Value),
        new("@Note", model.Note ?? (object)DBNull.Value),
        new("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value)
            };

            return _DbWorker.ExecuteNonQuery2("SP_InsertHotelShareHolderPayment", param);
        }


        public List<ShareHolderSearchViewModel> SearchShareHolder(string keyword)
        {
            var param = new[]
            {
        new SqlParameter("@Keyword", string.IsNullOrEmpty(keyword) ? (object)DBNull.Value : keyword)
    };

            return _DbWorker.GetDataTable("SP_SearchHotelShareHolder", param)
                .ToList<ShareHolderSearchViewModel>();
        }
        public List<ReportHotelShareHolderDetailViewModel> GetShareHolderDetail(int shareHolderId)
        {
            var param = new[]
            {
        new SqlParameter("@ShareHolderId", shareHolderId)
    };

            return _DbWorker
                .GetDataTable("SP_ReportHotelShareHolderDetail", param)
                .ToList<ReportHotelShareHolderDetailViewModel>();
        }


        public List<HotelShareHolderPaymentGridModel> GetListPayment(string name, int pageIndex, int pageSize)
        {
            SqlParameter[] param =
            {
            new("@ShareHolderName", name ?? (object)DBNull.Value),
            new("@PageIndex", pageIndex),
            new("@PageSize", pageSize)
        };

            return _DbWorker.GetDataTable("SP_GetListHotelShareHolderPayment", param)
                            .ToList<HotelShareHolderPaymentGridModel>();
        }

        public int DeletePayment(int id)
        {
            SqlParameter[] param = { new("@Id", id) };
            return _DbWorker.ExecuteNonQuery("SP_DeleteHotelShareHolderPayment", param);
        }

        /// <summary>
        /// Gợi ý danh sách Căn hộ (Hotel.IsApartment = 1) theo từ khóa.
        /// Dùng cho select2 ở ô "Tên Căn".
        /// </summary>
        /// <param name="term">Từ khóa tìm kiếm theo tên căn</param>
        /// <param name="size">Số lượng kết quả tối đa</param>
        public async Task<List<ApartmentSuggestViewModel>> Suggest(string term, int size = 20)
        {
            try
            {
                term = term?.Trim() ?? string.Empty;

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var query = _DbContext.Hotel
                        .AsNoTracking()
                        .Where(x => x.IsApartment == true); // chỉ lấy căn hộ

                    if (!string.IsNullOrEmpty(term))
                    {
                        query = query.Where(x => x.Name.Contains(term));
                    }

                    var data = await query
                        .OrderBy(x => x.Name)
                        .Take(size)
                        .Select(x => new ApartmentSuggestViewModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            Address =
                                (x.Street ?? "") +
                                (string.IsNullOrEmpty(x.City) ? "" : ", " + x.City) +
                                (string.IsNullOrEmpty(x.State) ? "" : ", " + x.State)
                        })
                        .ToListAsync();

                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Suggest - ApartmentDAL: " + ex);
                return new List<ApartmentSuggestViewModel>();
            }
        }
        public DataTable GetReportHotelShareHolder(ReportHotelShareHolderSearchModel model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
            new SqlParameter("@ShareHolderName", model.ShareHolderName ?? (object)DBNull.Value),
            new SqlParameter("@PageIndex", model.PageIndex),
            new SqlParameter("@PageSize", model.PageSize)
                };

                return _DbWorker.GetDataTable(
                    StoreProcedureConstant.SP_ReportHotelShareHolder,
                    objParam
                );
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetReportHotelShareHolder DAL: " + ex);
                return new DataTable();
            }
        }

        public DataTable GetLedgerByRoomId(int roomId)
        {
            SqlParameter[] param =
            {
            new SqlParameter("@RoomId", roomId)
        };

            return _DbWorker.GetDataTable("SP_Apartment_GetLedgerByRoomId", param);
        }
        public IEnumerable<HotelShareHolderReportModel> GetHotelShareHolderReport(
    int? userId, int pageIndex, int pageSize, out long total)
        {
            SqlParameter[] param =
            {
        new SqlParameter("@UserId", userId ?? (object)DBNull.Value),
        new SqlParameter("@PageIndex", pageIndex),
        new SqlParameter("@PageSize", pageSize),
        new SqlParameter("@Total", SqlDbType.BigInt) { Direction = ParameterDirection.Output }
    };

            var dt = _DbWorker.GetDataTable("SP_ReportHotelShareHolder", param);

            total = Convert.ToInt64(param[3].Value);
            return dt.ToList<HotelShareHolderReportModel>();
        }

        public int SaveLedger(ApartmentRoomLedgerModel model)
        {
            try
            {
                SqlParameter[] param = new SqlParameter[]
                {
            new SqlParameter("@Id",       model.Id ?? 0),
            new SqlParameter("@OrderId",  model.OrderId ?? 0),
            new SqlParameter("@HotelId",  model.HotelId ?? 0),
            new SqlParameter("@RoomId",   model.RoomId),

            new SqlParameter("@LedgerType", model.LedgerType),

            // ===== THU =====
            new SqlParameter("@CustomerName",    (object)model.CustomerName ?? DBNull.Value),
            new SqlParameter("@PhoneNumber",     (object)model.PhoneNumber ?? DBNull.Value),     // NEW
            new SqlParameter("@ContractDate",    (object)model.ContractDate ?? DBNull.Value),
            new SqlParameter("@ContractExpired", (object)model.ContractExpired ?? DBNull.Value),
            new SqlParameter("@DurationMonth",   (object)model.DurationMonth ?? DBNull.Value),   // NEW
            new SqlParameter("@RoomPrice",       (object)model.RoomPrice ?? DBNull.Value),
            new SqlParameter("@ServiceFee",      (object)model.ServiceFee ?? DBNull.Value),
            new SqlParameter("@RentAmount",      (object)model.RentAmount ?? DBNull.Value),      // NEW
            new SqlParameter("@DepositAmount",   (object)model.DepositAmount ?? DBNull.Value),   // NEW
            new SqlParameter("@TotalAmount",     (object)model.TotalAmount ?? DBNull.Value),
            new SqlParameter("@AmountPaid",      (object)model.AmountPaid ?? DBNull.Value),

            // ===== CHI =====
            new SqlParameter("@ExpenseTypeId", (object)model.ExpenseTypeId ?? DBNull.Value),
            new SqlParameter("@ExpenseType",   (object)model.ExpenseType ?? DBNull.Value),
            new SqlParameter("@Description",   (object)model.Description ?? DBNull.Value),
            new SqlParameter("@ExpenseAmount", (object)model.ExpenseAmount ?? DBNull.Value),
            new SqlParameter("@ExpenseDate",   (object)model.ExpenseDate ?? DBNull.Value),

            new SqlParameter("@UserId", model.CreatedBy)
                };

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_Apartment_SaveRoomLedger, param);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SaveLedger DAL error: " + ex);
                throw;
            }
        }


        public int DeleteLedger(int id, int userId)
        {
            SqlParameter[] param = new SqlParameter[]
            {
        new SqlParameter("@Id", id),
        new SqlParameter("@UserId", userId)
            };

            return _DbWorker.ExecuteNonQuery("SP_Apartment_DeleteRoomLedger", param);
        }





        ///// <summary>
        ///// Kiểm tra 1 căn hộ đã có đơn chưa (chưa bị IsDeleted).
        ///// Dùng để enforce rule: 1 căn chỉ được tạo 1 đơn.
        ///// </summary>
        //public async Task<bool> IsApartmentOrdered(int apartmentId)
        //{
        //    try
        //    {
        //        using (var _DbContext = new EntityDataContext(_connection))
        //        {
        //            return await _DbContext.ApartmentOrder
        //                .AsNoTracking()
        //                .AnyAsync(x => x.ApartmentId == apartmentId && !x.IsDeleted);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("IsApartmentOrdered - ApartmentOrderDAL: " + ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// Tạo mới 1 bản ghi ApartmentOrder.
        ///// </summary>
        //public async Task<int> CreateApartmentOrder(ApartmentOrder entity)
        //{
        //    try
        //    {
        //        using (var _DbContext = new EntityDataContext(_connection))
        //        {
        //            await _DbContext.ApartmentOrder.AddAsync(entity);
        //            await _DbContext.SaveChangesAsync();
        //            return entity.Id;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("CreateApartmentOrder - ApartmentOrderDAL: " + ex);
        //        return -1;
        //    }
        //}

        ///// <summary>
        ///// Lấy thông tin căn hộ (Hotel) theo Id, đảm bảo là IsApartment = true.
        ///// </summary>
        public async Task<Hotel> GetApartmentById(int apartmentId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Hotel
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == apartmentId && x.IsApartment == true);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetApartmentById - ApartmentOrderDAL: " + ex);
                return null;
            }
        }
    }
}
