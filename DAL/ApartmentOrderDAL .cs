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
        public DataTable GetLedgerByRoomId(int roomId)
        {
            SqlParameter[] param =
            {
            new SqlParameter("@RoomId", roomId)
        };

            return _DbWorker.GetDataTable("SP_Apartment_GetLedgerByRoomId", param);
        }
        public int SaveLedger(ApartmentRoomLedgerModel model)
        {
            try
            {
                SqlParameter[] param = new SqlParameter[]
                {
            new SqlParameter("@Id", model.Id ?? 0),
            new SqlParameter("@OrderId", model.OrderId ?? 0),
            new SqlParameter("@HotelId", model.HotelId ?? 0),
            new SqlParameter("@RoomId", model.RoomId),

            new SqlParameter("@LedgerType", model.LedgerType),

            // ===== THU =====
            new SqlParameter("@CustomerName", model.CustomerName ?? (object)DBNull.Value),
            new SqlParameter("@ContractDate", model.ContractDate ?? (object)DBNull.Value),
            new SqlParameter("@ContractExpired", model.ContractExpired ?? (object)DBNull.Value),
            new SqlParameter("@RoomPrice", model.RoomPrice ?? (object)DBNull.Value),
            new SqlParameter("@ServiceFee", model.ServiceFee ?? (object)DBNull.Value),
            new SqlParameter("@TotalAmount", model.TotalAmount ?? (object)DBNull.Value),
            new SqlParameter("@AmountPaid", model.AmountPaid ?? (object)DBNull.Value),

            // ===== CHI =====
            new SqlParameter("@ExpenseTypeId", model.ExpenseTypeId ?? (object)DBNull.Value),
            new SqlParameter("@ExpenseType", model.ExpenseType ?? (object)DBNull.Value),
            new SqlParameter("@Description", model.Description ?? (object)DBNull.Value),
            new SqlParameter("@ExpenseAmount", model.ExpenseAmount ?? (object)DBNull.Value),
            new SqlParameter("@ExpenseDate", model.ExpenseDate ?? (object)DBNull.Value),

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
