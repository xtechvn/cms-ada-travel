using DAL;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Apartment;
using Entities.ViewModels.Vinpearl;
using Microsoft.Extensions.Options;
using PdfSharp;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class ApartmentOrderRepository : IApartmentOrderRepository
    {
        private readonly ApartmentOrderDAL apartmentOrderDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;
        private readonly OrderDAL _OrderDal;
        private readonly ClientDAL _clientDAL;
        private readonly ContractPayDAL contractPayDAL;
        private readonly AllCodeDAL allCodeDAL;
        private readonly UserDAL userDAL;
        private readonly HotelBookingDAL hotelBookingDAL;
        private readonly HotelBookingCodeDAL _hotelBookingCodeDAL;
        private readonly ContractPayDAL _contractPayDAL;

        public ApartmentOrderRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            apartmentOrderDAL = new ApartmentOrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _OrderDal = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            allCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            userDAL = new UserDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            hotelBookingDAL = new HotelBookingDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _hotelBookingCodeDAL = new HotelBookingCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = dataBaseConfig;
        }

        /// <summary>
        /// Suggest căn hộ dùng cho select2
        /// </summary>
        public async Task<List<ApartmentSuggestViewModel>> Suggest(string term, int size = 20)
        {
            try
            {
                return await apartmentOrderDAL.Suggest(term, size);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Suggest - ApartmentOrderRepository: " + ex);
                return new List<ApartmentSuggestViewModel>();
            }
        }
        public async Task<GenericViewModel<OrderViewModel>> GetTotalCountOrder(OrderViewSearchModel searchModel, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<OrderViewModel>();
            try
            {
                DataTable dt = await _OrderDal.GetPagingList(searchModel, currentPage, pageSize, StoreProcedureConstant.GET_TOTALCOUNT_ORDER);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.TotalRecord = dt.Rows[0]["Total"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["Total"].ToString());
                    model.TotalRecord1 = dt.Rows[0]["TotalStatusTab1"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalStatusTab1"]);
                    model.TotalRecord2 = dt.Rows[0]["TotalStatusTab2"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalStatusTab2"]);
                    model.TotalRecord3 = dt.Rows[0]["TotalStatusTab3"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalStatusTab3"]);
                    model.TotalrecordErr = dt.Rows[0]["TotalStatusTab4"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalStatusTab4"]);
                    model.TotalRecord4 = dt.Rows[0]["TotalStatusTab5"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalStatusTab5"]);
                    model.Profit = dt.Rows[0]["Profit"].Equals(DBNull.Value) ? 0 : Convert.ToDouble(dt.Rows[0]["Profit"]);
                    model.Amount = dt.Rows[0]["Amount"].Equals(DBNull.Value) ? 0 : Convert.ToDouble(dt.Rows[0]["Amount"]);
                    model.Price = dt.Rows[0]["Price"].Equals(DBNull.Value) ? 0 : Convert.ToDouble(dt.Rows[0]["Price"]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalCountOrder in OrderRepository: " + ex);
            }
            return model;
        }
        private async Task<GenericViewModel<OrderViewModel>> GetOrders(OrderViewSearchModel searchModel, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<OrderViewModel>();
            try
            {
                DataTable dt = await _OrderDal.GetPagingList(searchModel, currentPage, pageSize, StoreProcedureConstant.GETALLORDER_SEARCH);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model.ListData = (from row in dt.AsEnumerable()
                                      select new OrderViewModel
                                      {
                                          OrderId = row["OrderId"].ToString(),
                                          OrderCode = row["OrderNo"].ToString(),
                                          StartDate = !row["StartDate"].Equals(DBNull.Value) ? Convert.ToDateTime(row["StartDate"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                                          EndDate = !row["EndDate"].Equals(DBNull.Value) ? Convert.ToDateTime(row["EndDate"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                                          ClientName = row["ClientName"].ToString(),
                                          ClientNumber = row["Phone"].ToString(),
                                          ClientEmail = row["Email"].ToString(),
                                          Note = row["Note"].ToString(),
                                          PaymentStatus = row["PaymentStatus"].ToString(),
                                          Payment = !row["Payment"].Equals(DBNull.Value) ? Convert.ToDouble(row["Payment"].ToString()) : 0,
                                          Amount = !row["Amount"].Equals(DBNull.Value) ? Convert.ToDouble(row["Amount"].ToString()) : 0,
                                          UtmSource = row["UtmSource"].ToString(),
                                          Profit = !row["Profit"].Equals(DBNull.Value) ? Convert.ToDouble(row["Profit"].ToString()) : 0,
                                          Status = row["Status"].ToString(),
                                          StatusCode = !row["StatusCode"].Equals(DBNull.Value) ? Convert.ToInt32(row["StatusCode"]) : -1,
                                          CreateDate = row["CreateTime"].Equals(DBNull.Value) ? "" : Convert.ToDateTime(row["CreateTime"]).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                                          CreateName = row["CreateName"].ToString(),
                                          UpdateName = row["UpdateName"].ToString(),
                                          UpdateDate = row["UpdateLast"].Equals(DBNull.Value) ? "" : Convert.ToDateTime(row["UpdateLast"]).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                                          SalerName = row["SalerName"].ToString(),
                                          ServiceType = row["ServiceType"].ToString(),
                                          SaleGroupName = row["SalerGroupName"].ToString(),
                                          Vouchercode = row["code"].ToString(),
                                          PaymentStatusName = row["PaymentStatusName"].ToString(),
                                          PermisionTypeName = row["PermisionTypeName"].ToString(),
                                          OperatorIdName = row["OperatorIdName"].ToString(),
                                          SalerUserName = row["SalerUserName"].ToString(),
                                          SalerEmail = row["SalerEmail"].ToString(),
                                          UtmMedium = row["UtmMedium"].ToString(),
                                          InvoiceRequestStatus = !row["InvoiceRequestStatus"].Equals(DBNull.Value) ? Convert.ToInt32(row["InvoiceRequestStatus"].ToString()) : 999,
                                          InvoiceRequestStatusName = row["InvoiceRequestStatusName"].ToString(),
                                          HotelId = row["HotelId"] == DBNull.Value ? 0 : Convert.ToInt32(row["HotelId"]),
                                          IsApartmentOrder = row["IsApartmentOrder"] == DBNull.Value ? false : Convert.ToBoolean(row["IsApartmentOrder"]),
                                          HotelName = row["HotelName"].ToString(),
                                          Street = row["Street"].ToString(),




                                      })// 🔥🔥🔥 CHỈ LẤY CĂN HỘ
                  .Where(x => x.IsApartmentOrder == true)
                  .ToList();

                    model.CurrentPage = currentPage;
                    model.PageSize = pageSize;
                    model.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrders in OrderRepository: " + ex);
            }
            return model;
        }
        public async Task<List<ApartmentRoomLedger>> LoadLedgerPopup(int roomId)
        {
            var dt = apartmentOrderDAL.GetLedgerByRoomId(roomId);
            return dt.ToList<ApartmentRoomLedger>();
        }
        public List<ReportHotelShareHolderViewModel> GetReportHotelShareHolder(
     ReportHotelShareHolderSearchModel model,
     out long total)
        {
            try
            {
                var dt = apartmentOrderDAL.GetReportHotelShareHolder(model);

                var data = dt.ToList<ReportHotelShareHolderViewModel>();

                total = data.Any() ? data.First().TotalRow : 0;

                return data;
            }
            catch
            {
                total = 0;
                throw;
            }
        }




        public async Task<int> SaveLedger(RoomLedgerInput model, int userId)
        {
            int result = 0;

            // ====== SAVE THU ======
            foreach (var t in model.Thu)
            {
                // --- TÍNH TOÁN SERVER-SIDE ---
                var duration = t.DurationMonth;                    // số tháng
                var roomPrice = t.RoomPrice;                       // giá phòng / tháng
                var serviceFee = t.ServiceFee;                     // phí DV
                var deposit = t.DepositAmount;                     // tiền cọc

                // Tiền nhà = giá phòng * thời hạn
                decimal rentAmount = roomPrice * duration;

                // Ngày hết hạn = Ngày HĐ + thời hạn (tháng)
                DateTime? contractExpired = null;
                if (t.ContractDate.HasValue && duration > 0)
                {
                    contractExpired = t.ContractDate.Value.AddMonths(duration);
                }

                // Tổng phải thu = tiền nhà + phí DV + cọc
                decimal totalAmount = rentAmount + serviceFee + deposit;

                var data = new ApartmentRoomLedgerModel()
                {
                    Id = t.Id,
                    RoomId = model.RoomId,
                    HotelId = model.HotelId,
                    LedgerType = 1,

                    CustomerName = t.CustomerName,
                    PhoneNumber = t.PhoneNumber,
                    ContractDate = t.ContractDate,
                    ContractExpired = contractExpired,

                    DurationMonth = duration,
                    RoomPrice = roomPrice,
                    ServiceFee = serviceFee,
                    RentAmount = rentAmount,
                    DepositAmount = deposit,
                    TotalAmount = totalAmount,            // Tổng phải thu mới
                    AmountPaid = t.TotalPaid,            // Tổng đã thanh toán

                    CreatedBy = userId
                };

                result = apartmentOrderDAL.SaveLedger(data);
            }

            // ====== SAVE CHI ======
            foreach (var c in model.Chi)
            {
                var data = new ApartmentRoomLedgerModel()
                {
                    Id = c.Id,
                    RoomId = model.RoomId,
                    HotelId = model.HotelId,
                    LedgerType = 2,

                    ExpenseTypeId = c.ExpenseTypeId,
                    ExpenseType = c.ExpenseType,
                    Description = c.Description,
                    ExpenseAmount = c.ExpenseAmount,
                    ExpenseDate = c.ExpenseDate,

                    CreatedBy = userId
                };

                result = apartmentOrderDAL.SaveLedger(data);
            }

            // Xóa các bản ghi bị remove trên UI
            if (model.DeletedIds != null && model.DeletedIds.Count > 0)
            {
                foreach (var id in model.DeletedIds)
                {
                    apartmentOrderDAL.DeleteLedger(id, userId);
                }
            }

            return result;
        }



        public async Task<GenericViewModel<OrderViewModel>> GetList(OrderViewSearchModel searchModel, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<OrderViewModel>();

            var model1 = await GetOrders(searchModel, currentPage, pageSize);
            var model3 = await GetTotalCountOrder(searchModel, currentPage, pageSize);

            try
            {
                if (model1.ListData != null)
                {
                    model.ListData = (from md1 in model1.ListData
                                      select new OrderViewModel
                                      {
                                          OrderId = md1.OrderId,
                                          OrderCode = md1.OrderCode,
                                          StartDate = md1.StartDate,
                                          EndDate = md1.EndDate,
                                          ClientName = md1.ClientName,
                                          ClientNumber = md1.ClientNumber,
                                          ClientEmail = md1.ClientEmail,
                                          Note = md1.Note,
                                          Payment = md1.Payment,
                                          Amount = md1.Amount,
                                          UtmSource = md1.UtmSource,
                                          Profit = md1.Profit,
                                          PaymentStatus = md1.PaymentStatus,
                                          //StatusDetail = md2.StatusDetail,
                                          Status = md1.Status,
                                          StatusCode = md1.StatusCode,
                                          CreateDate = md1.CreateDate,
                                          CreateName = md1.CreateName,
                                          UpdateName = md1.UpdateName,
                                          UpdateDate = md1.UpdateDate,
                                          SalerName = md1.SalerName,
                                          SaleGroupName = md1.SaleGroupName,
                                          PaymentStatusName = md1.PaymentStatusName,
                                          PermisionTypeName = md1.PermisionTypeName,
                                          Vouchercode = md1.Vouchercode,
                                          OperatorIdName = md1.OperatorIdName,
                                          SalerEmail = md1.SalerEmail,
                                          SalerUserName = md1.SalerUserName,
                                          UtmMedium = md1.UtmMedium,
                                          InvoiceRequestStatusName = md1.InvoiceRequestStatusName,
                                          InvoiceRequestStatus = md1.InvoiceRequestStatus,
                                          Street = md1.Street,
                                          IsApartmentOrder = md1.IsApartmentOrder,
                                          HotelName = md1.HotelName,
                                          HotelId = md1.HotelId

                                      }
                                                    ).ToList();
                }
                else
                {
                    //  LogHelper.InsertLogTelegram("GetList -  OrderRepository: No Order Count with" + JsonConvert.SerializeObject(searchModel));

                }
                model.CurrentPage = model1.CurrentPage;
                model.PageSize = model1.PageSize;
                model.TotalPage = model1.TotalPage;
                model.TotalRecord = model3.TotalRecord;
                model.TotalRecord1 = model3.TotalRecord1;
                model.TotalRecord2 = model3.TotalRecord2;
                model.TotalRecord3 = model3.TotalRecord3;
                model.TotalRecord4 = model3.TotalRecord4;
                model.Amount = model3.Amount;
                model.Price = model3.Price;
                model.Profit = model3.Profit;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList in OrderRepository: " + ex);
            }
            return model;
        }
        public List<HotelShareHolderPaymentGridModel> GetListPayment(string name, int pageIndex, int pageSize, out long total)
        {
            var data = apartmentOrderDAL.GetListPayment(name, pageIndex, pageSize);
            total = data.Any() ? data.First().TotalRow : 0;
            return data;
        }

     public List<ReportHotelShareHolderDetailViewModel> GetShareHolderDetail(int shareHolderId)
        {
            var data = apartmentOrderDAL.GetShareHolderDetail(shareHolderId);
            
            return data;
        }
        public int InsertPayment(HotelShareHolderPayment model)
        {
            return apartmentOrderDAL.InsertPayment(model);
        }

        public int DeletePayment(int id)
        {
            return apartmentOrderDAL.DeletePayment(id);
        }
        public List<ShareHolderSearchViewModel> SearchShareHolder(string keyword)
        {
            return apartmentOrderDAL.SearchShareHolder(keyword);
        }


        ///// <summary>
        ///// Tạo đơn căn hộ
        ///// </summary>
        //public async Task<int> Create(ApartmentOrderCreateViewModel model, int currentUserId)
        //{
        //    try
        //    {
        //        // 1️⃣ Check căn hộ có tồn tại
        //        var apartment = await apartmentOrderDAL.GetApartmentById(model.ApartmentId);
        //        if (apartment == null)
        //            throw new Exception("Căn hộ không tồn tại hoặc không phải loại Apartment");

        //        // 2️⃣ Check 1 căn chỉ có 1 đơn
        //        var existed = await apartmentOrderDAL.IsApartmentOrdered(model.ApartmentId);
        //        if (existed)
        //            throw new Exception("Căn hộ này đã được tạo đơn trước đó, không thể tạo thêm.");

        //        var entity = new ApartmentOrder
        //        {
        //            ApartmentId = model.ApartmentId,
        //            ApartmentName = string.IsNullOrWhiteSpace(model.ApartmentName)
        //                            ? apartment.Name
        //                            : model.ApartmentName,
        //            Address = string.IsNullOrWhiteSpace(model.Address)
        //                      ? BuildAddress(apartment)
        //                      : model.Address,
        //            CreatedDate = DateTime.Now,
        //            CreatedBy = currentUserId,
        //            IsDeleted = false
        //        };

        //        return await apartmentOrderDAL.CreateApartmentOrder(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("Create - ApartmentOrderRepository: " + ex);
        //        return -1;
        //    }
        //}


    }
}
