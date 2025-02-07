using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Entities.ViewModels.OrderManual;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using static TheArtOfDev.HtmlRenderer.Adapters.RGraphicsPath;

namespace DAL
{
    public class OrderDAL : GenericService<Order>
    {
        private static DbWorker _DbWorker;
        public OrderDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);

        }

        public async Task<DataTable> GetPagingList(OrderViewSearchModel searchModel, int currentPage, int pageSize, string proc)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[25];


                objParam[0] = (CheckDate(searchModel.CreateTime) == DateTime.MinValue) ? new SqlParameter("@CreateTime", DBNull.Value) : new SqlParameter("@CreateTime", CheckDate(searchModel.CreateTime));
                objParam[1] = (CheckDate(searchModel.ToDateTime) == DateTime.MinValue) ? new SqlParameter("@ToDateTime", DBNull.Value) : new SqlParameter("@ToDateTime", CheckDate(searchModel.ToDateTime).AddDays(1));
                objParam[2] = new SqlParameter("@SysTemType", searchModel.SysTemType);
                objParam[3] = new SqlParameter("@OrderNo", searchModel.OrderNo);
                objParam[4] = new SqlParameter("@Note", searchModel.Note);
                objParam[5] = new SqlParameter("@ServiceType", searchModel.ServiceType == null ? "" : string.Join(",", searchModel.ServiceType));
                objParam[6] = new SqlParameter("@UtmSource", searchModel.UtmSource == null ? "" : searchModel.UtmSource);
                objParam[7] = new SqlParameter("@status", searchModel.Status == null ? "" : string.Join(",", searchModel.Status));
                objParam[8] = new SqlParameter("@CreateName", searchModel.CreateName);
                if (searchModel.Sale == null)
                {
                    objParam[9] = new SqlParameter("@Sale", DBNull.Value);

                }
                else
                {
                    objParam[9] = new SqlParameter("@Sale", searchModel.Sale);

                }
                objParam[10] = new SqlParameter("@SaleGroup", searchModel.SaleGroup);
                objParam[11] = new SqlParameter("@PageIndex", currentPage);
                objParam[12] = new SqlParameter("@PageSize", pageSize);
                objParam[13] = new SqlParameter("@StatusTab", searchModel.StatusTab);
                objParam[14] = new SqlParameter("@ClientId", searchModel.ClientId);
                objParam[15] = new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                objParam[16] = new SqlParameter("@OperatorId", searchModel.OperatorId);
                if (searchModel.StartDateFrom == null)
                {
                    objParam[17] = new SqlParameter("@StartDateFrom", DBNull.Value);

                }
                else
                {
                    objParam[17] = new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);

                }
                if (searchModel.StartDateTo == null)
                {
                    objParam[18] = new SqlParameter("@StartDateTo", DBNull.Value);

                }
                else
                {
                    objParam[18] = new SqlParameter("@StartDateTo", searchModel.StartDateTo);

                }
                if (searchModel.EndDateFrom == null)
                {
                    objParam[19] = new SqlParameter("@EndDateFrom", DBNull.Value);

                }
                else
                {
                    objParam[19] = new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);

                }
                if (searchModel.EndDateTo == null)
                {
                    objParam[20] = new SqlParameter("@EndDateTo", DBNull.Value);

                }
                else
                {
                    objParam[20] = new SqlParameter("@EndDateTo", searchModel.EndDateTo);

                }
                if (searchModel.PermisionType == null)
                {
                    objParam[21] = new SqlParameter("@PermisionType", DBNull.Value);

                }
                else
                {
                    objParam[21] = new SqlParameter("@PermisionType", searchModel.PermisionType);

                }
                if (searchModel.PaymentStatus == null)
                {
                    objParam[22] = new SqlParameter("@PaymentStatus", DBNull.Value);

                }
                else
                {
                    objParam[22] = new SqlParameter("@PaymentStatus", searchModel.PaymentStatus);

                }

                objParam[23] = new SqlParameter("@OrderId", searchModel.BoongKingCode);
                objParam[24] = new SqlParameter("@TenantId", (searchModel.TenantId == null ? DBNull.Value : (int)searchModel.TenantId));


                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - OrderDal: " + ex);
            }
            return null;
        }
        private DateTime CheckDate(string dateTime)
        {
            DateTime _date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateTime))
            {
                _date = DateTime.ParseExact(dateTime, "d/M/yyyy", CultureInfo.InvariantCulture);
            }

            return _date != DateTime.MinValue ? _date : DateTime.MinValue;
        }
        public Order GetByOrderId(long OrderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().FirstOrDefault(s => s.OrderId == OrderId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderId - OrderDal: " + ex);
                return null;
            }
        }

        public List<Order> GetByOrderIds(List<long> orderIds)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().Where(s => orderIds.Contains(s.OrderId)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderIds - OrderDal: " + ex);
                return new List<Order>();
            }
        }
        public List<Order> GetByOrderNos(List<string> orderNos)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().Where(s => orderNos.Contains(s.OrderNo)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderNos - OrderDal: " + ex);
                return new List<Order>();
            }
        }
        public List<Order> GetByClientId(long Client_Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().Where(s => s.ClientId == Client_Id).OrderByDescending(s => s.CreateTime).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - OrderDal: " + ex);
                return null;
            }
        }
        public async Task<long> CreateOrder(Order order)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (order.OrderNo == null || order.OrderNo.Trim() == "")
                    {
                        return -1;

                    }
                    else
                    {
                        _DbContext.Order.Add(order);
                        await _DbContext.SaveChangesAsync();
                        return order.OrderId;
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateOrder - OrderDal: " + ex);
                return -2;
            }
        }
        public long CountOrderInYear()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.Order.AsNoTracking().Where(x => (x.CreateTime ?? DateTime.Now).Year == DateTime.Now.Year).Count();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountOrderInYear - OrderDAL: " + ex.ToString());
                return -1;
            }
        }
        public static async Task<string> getOrderNoByOrderNo(string order_no)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var data = _DbContext.Order.AsNoTracking().FirstOrDefault(s => s.OrderNo == order_no);
                    return data == null ? "" : data.OrderNo;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderNoByOrderNo - OrderDAL: " + ex);
                return "";
            }
        }
        public Order GetByOrderNo(string orderNo)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().FirstOrDefault(s => s.OrderNo == orderNo);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderNo - OrderDal: " + ex);
                return null;
            }
        }


        public DataTable GetListOrderByClientId(long clienId, string proc, int status = 0)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@ClientId", clienId);
                objParam[1] = new SqlParameter("@IsFinishPayment", DBNull.Value);
                if (status == 0)
                    objParam[2] = new SqlParameter("@OrderStatus", DBNull.Value);
                else
                    objParam[2] = new SqlParameter("@OrderStatus", status);

                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListOrderByClientId - OrderDal: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetDetailOrderServiceByOrderId(int OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailOrderServiceByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderServiceByOrderId - OrderDal: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetDetailOrderByOrderId(int OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailOrderByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SP_GetDetailOrderByOrderId - OrderDal: " + ex);
            }
            return null;
        }

        public async Task<double> UpdateOrderDetail(long OrderId, long user_id)
        {
            try
            {
                List<int> order_status_not_allowed = new List<int>() { (int)OrderStatus.FINISHED, (int)OrderStatus.WAITING_FOR_ACCOUNTANT, (int)OrderStatus.CANCEL, (int)OrderStatus.ACCOUNTANT_DECLINE, (int)OrderStatus.CREATED_ORDER, (int)OrderStatus.CONFIRMED_SALE };

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data = _DbContext.Order.AsNoTracking().FirstOrDefault(s => s.OrderId == OrderId);
                    double amount = 0;
                    double price = 0;
                    double discount = 0;
                    double profit = 0;
                    double commission = 0;
                    double FundCustomerCare = 0;
                    List<int> product_service = new List<int>();
                    data.StartDate = null;
                    data.EndDate = null;
                    List<int> other_servicetype_main = new List<int>() { (int)ServiceOtherType.WaterSport };
                    if (data != null && data.OrderId > 0)
                    {
                        if (data.VoucherId != null && data.VoucherId > 0)
                        {
                            amount -= (data.Discount == null ? 0 : (double)data.Discount);
                        }
                        var order_status_old = data.OrderStatus;
                        var list_other_booking_all = await _DbContext.OtherBooking.AsNoTracking().Where(s => s.OrderId == OrderId).ToListAsync();
                        var vinWonderBookings_all = await _DbContext.VinWonderBooking.AsNoTracking().Where(s => s.OrderId == OrderId).ToListAsync();
                        var list_tour_booking_all = await _DbContext.Tour.AsNoTracking().Where(s => s.OrderId == OrderId).ToListAsync();
                        var list_flybooking_all = await _DbContext.FlyBookingDetail.AsNoTracking().Where(s => s.OrderId == OrderId).ToListAsync();
                        var list_hotel_booking_all = await _DbContext.HotelBooking.AsNoTracking().Where(s => s.OrderId == OrderId).ToListAsync();
                        var list_watersport = await _DbContext.OtherBooking.AsNoTracking().Where(s => s.OrderId == OrderId && s.ServiceType > 0 && s.ServiceType == (int)ServiceOtherType.WaterSport).ToListAsync();


                        var list_flybooking = list_flybooking_all.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                        var list_flybooking_id = list_flybooking.Select(x => x.Id);
                        var list_flybooking_optional = await _DbContext.FlyBookingPackagesOptional.AsNoTracking().Where(s => list_flybooking_id.Contains(s.BookingId) && s.Status != 1).ToListAsync();
                        if (list_flybooking != null && list_flybooking.Count > 0)
                        {
                            amount += list_flybooking.Sum(x => x.Amount);
                            if (list_flybooking_optional != null && list_flybooking_optional.Count > 0)
                            {
                                price += list_flybooking_optional.Sum(x => x.Amount);
                            }
                            else
                            {
                                price += list_flybooking.Sum(x => x.Amount + (x.TotalDiscount != null ? (double)x.TotalDiscount : 0) - (x.Profit != null ? (double)x.Profit : 0)); //- (x.Adgcommission != null ? (double)x.Adgcommission : 0) - (x.OthersAmount != null ? (double)x.OthersAmount : 0));
                            }
                            //discount += list_flybooking.Sum(x => x.TotalDiscount != null ? (double)x.TotalDiscount : 0);
                            profit += list_flybooking.Sum(x => x.Profit != null ? (double)x.Profit : 0);
                            product_service.Add((int)ServicesType.FlyingTicket);
                            var min_date = list_flybooking.OrderBy(x => x.StartDate).FirstOrDefault();
                            var max_date = list_flybooking.OrderByDescending(x => x.EndDate).FirstOrDefault();
                            if (data.StartDate == null || data.StartDate > min_date.StartDate) data.StartDate = min_date.StartDate;
                            if (data.EndDate == null || data.EndDate < max_date.EndDate) data.EndDate = max_date.EndDate;
                            commission += list_flybooking.Sum(x => x.Adgcommission != null ? (double)x.Adgcommission : 0);

                        }

                        var list_hotel_booking = list_hotel_booking_all.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                        var list_hotel_booking_id = list_hotel_booking.Select(x => x.Id);
                        var list_hotel_optional = await _DbContext.HotelBookingRoomsOptional.AsNoTracking().Where(s => list_hotel_booking_id.Contains(s.HotelBookingId) && s.Status != 1).ToListAsync();
                        if (list_hotel_booking != null && list_hotel_booking.Count > 0)
                        {
                            amount += list_hotel_booking.Sum(x => x.TotalAmount);
                            if (list_hotel_optional != null && list_hotel_optional.Count > 0)
                            {
                                price += list_hotel_optional.Where(x => x.IsRoomFund == null || x.IsRoomFund == false).Sum(x => x.TotalAmount);
                            }
                            else
                            {
                                price += list_hotel_booking.Sum(x => x.TotalAmount - (double)x.TotalProfit);  // - (x.TotalDiscount != null ? (double)x.TotalDiscount : 0) - (x.TotalOthersAmount != null ? (double)x.TotalOthersAmount : 0));
                            }
                            // discount += list_hotel_booking.Sum(x => x.TotalDiscount != null ? (double)x.TotalDiscount : 0);
                            profit += list_hotel_booking.Sum(x => x.TotalProfit);
                            product_service.Add((int)ServicesType.VINHotelRent);
                            var min_date = list_hotel_booking.OrderBy(x => x.ArrivalDate).FirstOrDefault();
                            var max_date = list_hotel_booking.OrderByDescending(x => x.DepartureDate).FirstOrDefault();
                            if (data.StartDate == null || data.StartDate > min_date.ArrivalDate) data.StartDate = min_date.ArrivalDate;
                            if (data.EndDate == null || data.EndDate < max_date.DepartureDate) data.EndDate = max_date.DepartureDate;
                            commission += list_hotel_booking.Sum(x => x.TotalDiscount != null ? (double)x.TotalDiscount : 0);

                        }
                        var hotel_extra = await _DbContext.HotelBookingRoomExtraPackages.AsNoTracking().Where(s => list_hotel_booking_id.Contains((long)s.HotelBookingId)).ToListAsync();
                        if (hotel_extra != null && hotel_extra.Count > 0)
                        {
                            price += hotel_extra.Sum(x => x.UnitPrice != null ? (double)x.UnitPrice : 0);
                        }

                        var list_tour_booking = list_tour_booking_all.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                        var list_tour_id = list_tour_booking.Select(x => x.Id);
                        var list_tour_optional = await _DbContext.TourPackagesOptional.AsNoTracking().Where(s => list_tour_id.Contains(s.TourId != null ? (long)s.TourId : 0) && s.Status != 1).ToListAsync();
                        if (list_tour_booking != null && list_tour_booking.Count > 0)
                        {
                            amount += list_tour_booking.Sum(x => x.Amount != null ? (double)x.Amount : 0);
                            FundCustomerCare += list_tour_booking.Sum(x => x.FundCustomerCare != null ? (double)x.FundCustomerCare : 0);
                            if (list_tour_optional != null && list_tour_optional.Count > 0)
                            {
                                price += list_tour_optional.Sum(x => x.Amount != null ? (double)x.Amount : 0);
                            }
                            else
                            {
                                price += list_tour_booking.Sum(x => x.Amount != null ? (double)x.Amount - (x.Profit != null ? (double)x.Profit : 0) : 0); // - (x.Commission != null ? (double)x.Commission : 0) - (x.OthersAmount != null ? (double)x.OthersAmount : 0) : 0);
                            }

                            // discount += list_tour_booking.Sum(x => x.TotalDiscount != null ? (double)x.TotalDiscount : 0);
                            profit += list_tour_booking.Sum(x => x.Profit != null ? (double)x.Profit : 0);
                            product_service.Add((int)ServicesType.Tourist);
                            var min_date = list_tour_booking.OrderBy(x => x.StartDate).FirstOrDefault();
                            var max_date = list_tour_booking.OrderByDescending(x => x.EndDate).FirstOrDefault();
                            if (data.StartDate == null || data.StartDate > min_date.StartDate) data.StartDate = min_date.StartDate;
                            if (data.EndDate == null || data.EndDate < max_date.EndDate) data.EndDate = max_date.EndDate;
                            commission += list_tour_booking.Sum(x => x.Commission != null ? (double)x.Commission : 0);

                        }

                        var vinWonderBookings = vinWonderBookings_all.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                        if (vinWonderBookings != null && vinWonderBookings.Count > 0)
                        {
                            amount += vinWonderBookings.Sum(x => x.Amount != null ? (double)x.Amount : 0);
                            // price += vinWonderBookings.Sum(x => (x.Amount != null ? (double)x.Amount : 0) - (x.TotalProfit != null ? (double)x.TotalProfit : 0));
                            // discount += list_other_booking.Sum(x => x.TotalDiscount != null ? (double)x.TotalDiscount : 0);
                            profit += vinWonderBookings.Sum(x => (x.TotalProfit != null ? (double)x.TotalProfit : 0));
                            price += vinWonderBookings.Sum(x => x.TotalUnitPrice == null ? ((double)x.Amount - (double)x.TotalProfit) : (double)x.TotalUnitPrice); //- (x.Commission != null ? (double)x.Commission : 0) - (x.OthersAmount != null ? (double)x.OthersAmount : 0)) : (double)x.TotalUnitPrice);
                            product_service.Add((int)ServicesType.VinWonder);
                            var min_date = vinWonderBookings.OrderBy(x => x.CreatedDate).FirstOrDefault();
                            var max_date = vinWonderBookings.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                            var VinWonderBooking_Ticket_min_date = await _DbContext.VinWonderBookingTicket.FirstOrDefaultAsync(s => s.BookingId == min_date.Id);
                            var VinWonderBooking_Ticket_max_date = await _DbContext.VinWonderBookingTicket.FirstOrDefaultAsync(s => s.BookingId == max_date.Id);

                            if (data.StartDate == null || data.StartDate > min_date.CreatedDate) data.StartDate = VinWonderBooking_Ticket_min_date.DateUsed;
                            if (data.EndDate == null || data.EndDate < max_date.CreatedDate) data.EndDate = VinWonderBooking_Ticket_max_date.DateUsed;
                            commission += vinWonderBookings.Sum(x => x.Commission != null ? (double)x.Commission : 0);


                        }
                        var list_other_booking = list_other_booking_all.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                        var list_other_id = list_other_booking.Select(x => x.Id);
                        var list_other_optional = await _DbContext.OtherBookingPackagesOptional.AsNoTracking().Where(s => list_other_id.Contains(s.BookingId) && s.Status != 1).ToListAsync();
                        if (list_other_booking != null && list_other_booking.Count > 0)
                        {
                            amount += list_other_booking.Sum(x => x.Amount);
                            if (list_other_optional != null && list_other_optional.Count > 0)
                            {
                                price += list_other_optional.Sum(x => x.Amount);
                            }
                            else
                            {
                                price += list_other_booking.Sum(x => x.Amount - x.Profit);// - (x.Commission != null ? (double)x.Commission : 0) - (x.OthersAmount != null ? (double)x.OthersAmount : 0));
                            }
                            // discount += list_other_booking.Sum(x => x.TotalDiscount != null ? (double)x.TotalDiscount : 0);
                            profit += list_other_booking.Sum(x => x.Profit);
                            product_service.Add((int)ServicesType.Other);
                            var min_date = list_other_booking.OrderBy(x => x.StartDate).FirstOrDefault();
                            var max_date = list_other_booking.OrderByDescending(x => x.EndDate).FirstOrDefault();
                            if (data.StartDate == null || data.StartDate > min_date.StartDate) data.StartDate = min_date.StartDate;
                            if (data.EndDate == null || data.EndDate < max_date.EndDate) data.EndDate = max_date.EndDate;
                            commission += list_other_booking.Sum(x => x.Commission != null ? (double)x.Commission : 0);

                        }

                        var list_ws_booking = list_watersport.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                        if (list_ws_booking != null && list_ws_booking.Count > 0)
                        {
                            product_service.Add((int)ServicesType.WaterSport);
                        }
                        DataTable contract_pay_list = await GetContractPayByOrderId(OrderId);
                        var listData_contract_pay = contract_pay_list.ToList<ContractPayDetaiByOrderIdlViewModel>();
                        double contract_pay_total_amount = listData_contract_pay.Sum(x => x.AmountPay);
                        if (amount > contract_pay_total_amount && data.PaymentStatus == (int)PaymentStatus.PAID)
                        {
                            data.PaymentStatus = (int)PaymentStatus.PAID_NOT_ENOUGH;
                            data.IsFinishPayment = 0;
                        }
                        data.Amount = amount;
                        data.Price = price;
                        data.Profit = profit - (data.Discount == null ? 0 : data.Discount);
                        data.TotalFundCustomerCare = FundCustomerCare;
                        //data.Discount = discount;
                        data.Commission = commission;
                        data.ProductService = string.Join(",", product_service);
                        data.UpdateLast = DateTime.Now;
                        data.UserUpdateId = user_id;
                        if (data.StartDate == null) data.StartDate = DateTime.Now;
                        if (data.EndDate == null) data.EndDate = DateTime.Now.AddHours(2);
                        // Update Order Status:
                        bool status_confirm = false;
                        //-- Case CANCEL:
                        bool has_other_than_cancel = (list_flybooking_all != null && list_flybooking_all.Count > 0 && list_flybooking_all.Any(x => x.Status != (int)ServiceStatus.Cancel))
                            || (list_hotel_booking_all != null && list_hotel_booking_all.Count > 0 && list_hotel_booking_all.Any(x => x.Status != (int)ServiceStatus.Cancel))
                            || (list_tour_booking_all != null && list_tour_booking_all.Count > 0 && list_tour_booking_all.Any(x => x.Status != (int)ServiceStatus.Cancel))
                            || (vinWonderBookings_all != null && vinWonderBookings_all.Count > 0 && vinWonderBookings_all.Any(x => x.Status != (int)ServiceStatus.Cancel))
                            || (list_other_booking_all != null && list_other_booking_all.Count > 0 && list_other_booking_all.Any(x => x.Status != (int)ServiceStatus.Cancel));
                        bool atleast_has_one = (list_flybooking_all != null && list_flybooking_all.Count > 0)
                           || (list_hotel_booking_all != null && list_hotel_booking_all.Count > 0)
                           || (list_tour_booking_all != null && list_tour_booking_all.Count > 0)
                           || (vinWonderBookings_all != null && vinWonderBookings_all.Count > 0)
                           || (list_other_booking_all != null && list_other_booking_all.Count > 0);

                        if (!has_other_than_cancel && atleast_has_one && order_status_old == (int)OrderStatus.OPERATOR_DECLINE)
                        {
                            data.OrderStatus = (int)OrderStatus.CANCEL;
                            status_confirm = true;
                        }
                        //-- Case Waiting Accountant:
                        if (!status_confirm)
                        {
                            bool has_other_than_payment = (list_flybooking_all != null && list_flybooking_all.Count > 0 && list_flybooking_all.Any(x => x.Status != (int)ServiceStatus.Payment && x.Status != (int)ServiceStatus.Cancel))
                          || (list_hotel_booking_all != null && list_hotel_booking_all.Count > 0 && list_hotel_booking_all.Any(x => x.Status != (int)ServiceStatus.Payment && x.Status != (int)ServiceStatus.Cancel))
                          || (list_tour_booking_all != null && list_tour_booking_all.Count > 0 && list_tour_booking_all.Any(x => x.Status != (int)ServiceStatus.Payment && x.Status != (int)ServiceStatus.Cancel))
                          || (vinWonderBookings_all != null && vinWonderBookings_all.Count > 0 && vinWonderBookings_all.Any(x => x.Status != (int)ServiceStatus.Payment && x.Status != (int)ServiceStatus.Cancel))
                          || (list_other_booking_all != null && list_other_booking_all.Count > 0 && list_other_booking_all.Any(x => x.Status != (int)ServiceStatus.Payment && x.Status != (int)ServiceStatus.Cancel));
                            atleast_has_one = false;
                            atleast_has_one = (list_flybooking != null && list_flybooking.Count > 0)
                               || (list_hotel_booking != null && list_hotel_booking.Count > 0)
                               || (list_tour_booking != null && list_tour_booking.Count > 0)
                               || (vinWonderBookings != null && vinWonderBookings.Count > 0)
                               || (list_other_booking != null && list_other_booking.Count > 0);

                            if (!has_other_than_payment && atleast_has_one && order_status_old == (int)OrderStatus.WAITING_FOR_OPERATOR)
                            {
                                data.OrderStatus = (int)OrderStatus.WAITING_FOR_ACCOUNTANT;
                                status_confirm = true;
                            }
                        }

                        _DbContext.Order.Update(data);
                        await _DbContext.SaveChangesAsync();
                        UpdateOrderOperator(OrderId);
                        var ListOrderBookClosing = await GetListOrderBookClosingByOrderId(OrderId);
                        DataTable dt = ListOrderBookClosing;
                        if (dt != null && dt.Rows.Count > 0)
                        {

                            var dataOrderBookClosing = dt.ToList<OrderBookClosingRequestViewModel>();

                            var OrderBookClosingModel = new OrderBookClosingViewModel
                            {
                                FromDateStr = dataOrderBookClosing[0].FromDate.ToString("dd/MM/yyyy"),
                                ToDateStr = dataOrderBookClosing[0].ToDate.ToString("dd/MM/yyyy"),
                                UserFinalize = (long)data.UserUpdateId,
                            };
                            var date = DateUtil.StringToDate(OrderBookClosingModel.ToDateStr);
                            OrderBookClosingModel.ToDate = ((DateTime)date).AddHours(23).AddMinutes(59).AddSeconds(59);
                            await OrderBookClosing(OrderBookClosingModel);
                            await UpdateBookClosingByOrderId(OrderId, 0, (long)data.UserUpdateId);
                        }


                        return amount;
                    }
                    else return -1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderAmount - OrderDal: " + ex);
            }
            return -2;

        }

        public async Task<int> UpdateOrderStatus(long OrderId, long Status, long UpdatedBy, long UserVerify)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                objParam[1] = new SqlParameter("@Status", Status);
                objParam[2] = new SqlParameter("@UpdatedBy", UpdatedBy);
                objParam[3] = UserVerify == 0 ? new SqlParameter("@UserVerify", DBNull.Value) : new SqlParameter("@UserVerify", UserVerify);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderStatus, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderServiceByOrderId - OrderDal: " + ex);
            }
            return 0;
        }

        public async Task<DataTable> GetAllServiceByOrderId(long OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAllServiceByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllServiceByOrderId - OrderDal: " + ex);
            }
            return null;
        }
        public int InsertOrder(Order order)
        {
            try
            {
                SqlParameter[] objParam_order = new SqlParameter[]
                    {
                        new SqlParameter("@OrderNo", order.OrderNo ?? (object)DBNull.Value),
                    new SqlParameter("@ServiceType", order.ServiceType ?? (object)DBNull.Value),
                    new SqlParameter("@CreateTime", order.CreateTime ?? (object)DBNull.Value),
                                new SqlParameter("@Amount", order.Amount ?? (object)DBNull.Value),
                    new SqlParameter("@PaymentStatus", order.PaymentStatus ?? (object)DBNull.Value),
                    new SqlParameter("@ClientId", order.ClientId ?? (object)DBNull.Value),
                    new SqlParameter("@ContactClientId", order.ContactClientId ?? (object)DBNull.Value),
                    new SqlParameter("@OrderStatus", order.OrderStatus ?? (object)DBNull.Value),
                    new SqlParameter("@ContractId", order.ContractId ?? (object)DBNull.Value),
                    new SqlParameter("@SmsContent", order.SmsContent ?? (object)DBNull.Value),
                    new SqlParameter("@PaymentType", order.PaymentType ?? (object)DBNull.Value),
                    new SqlParameter("@BankCode", order.BankCode ?? (object)DBNull.Value),
                    new SqlParameter("@PaymentDate", order.PaymentDate ?? (object)DBNull.Value),
                    new SqlParameter("@PaymentNo", order.PaymentNo ?? (object)DBNull.Value),
                    new SqlParameter("@ColorCode", order.ColorCode ?? (object)DBNull.Value),
                    new SqlParameter("@Discount", order.Discount ?? (object)DBNull.Value),
                    new SqlParameter("@Profit", order.Profit ?? (object)DBNull.Value),
                    new SqlParameter("@ExpriryDate", order.ExpriryDate ?? (object)DBNull.Value),
                    new SqlParameter("@StartDate", order.StartDate ?? (object)DBNull.Value),
                    new SqlParameter("@EndDate", order.EndDate ?? (object)DBNull.Value),
                    new SqlParameter("@ProductService", order.ProductService ?? (object)DBNull.Value),
                    new SqlParameter("@Note", order.Note ?? (object)DBNull.Value),
                    new SqlParameter("@UtmSource", order.UtmSource ?? (object)DBNull.Value),
                    new SqlParameter("@UpdateLast", order.UpdateLast ?? (object)DBNull.Value),
                    new SqlParameter("@SalerId", order.SalerId ?? (object)DBNull.Value),
                    new SqlParameter("@SalerGroupId", order.SalerGroupId ?? (object)DBNull.Value),
                    new SqlParameter("@UserUpdateId", order.UserUpdateId ?? (object)DBNull.Value),
                    new SqlParameter("@SystemType", order.SystemType ?? (object)DBNull.Value),
                    new SqlParameter("@AccountClientId", order.AccountClientId ?? (object)DBNull.Value),
                    new SqlParameter("@CreatedBy", order.CreatedBy ?? (object)DBNull.Value),
                    new SqlParameter("@Description", order.Description ?? (object)DBNull.Value),
                    new SqlParameter("@BranchCode", order.BranchCode ?? (object)DBNull.Value),
                    new SqlParameter("@BookingInfo", order.BookingInfo ?? (object)DBNull.Value),
                    new SqlParameter("@Label", order.Label ?? (object)DBNull.Value),
                    new SqlParameter("@IsFinishPayment", order.IsFinishPayment ?? (object)DBNull.Value),
                    new SqlParameter("@PercentDecrease", order.PercentDecrease ?? (object)DBNull.Value),
                    new SqlParameter("@VoucherId", order.VoucherId ?? (object)DBNull.Value),
                    new SqlParameter("@Price", order.Price ?? (object)DBNull.Value),
                    new SqlParameter("@SupplierId", order.SupplierId ?? (object)DBNull.Value),
                    new SqlParameter("@DepartmentId", order.DepartmentId ?? (object)DBNull.Value),
                    new SqlParameter("@OperatorId", order.OperatorId ?? (object)DBNull.Value),
                    new SqlParameter("@UserVerify", order.UserVerify ?? (object)DBNull.Value),
                    new SqlParameter("@VerifyDate", order.VerifyDate ?? (object)DBNull.Value),
                    new SqlParameter("@DebtStatus", order.DebtStatus ?? (object)DBNull.Value),
                    new SqlParameter("@DebtNote", order.DebtNote ?? (object)DBNull.Value),
                    new SqlParameter("@Commission", order.Commission ?? (object)DBNull.Value),
                    new SqlParameter("@UtmMedium", order.UtmMedium ?? (object)DBNull.Value),
                    new SqlParameter("@Refund", order.Refund ?? (object)DBNull.Value),
                    new SqlParameter("@TotalFundCustomerCare", order.TotalFundCustomerCare ?? (object)DBNull.Value),
                    new SqlParameter("@AmountFinalize", order.AmountFinalize ?? (object)DBNull.Value),
                    new SqlParameter("@FinalizeDate", order.FinalizeDate ?? (object)DBNull.Value),
                    new SqlParameter("@IsLock", order.IsLock ?? (object)DBNull.Value),
                    new SqlParameter("@ClosingEndDate", order.ClosingEndDate ?? (object)DBNull.Value),
                    new SqlParameter("@TenantId", order.TenantId ?? (object)DBNull.Value),
                };

                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.InsertOrder, objParam_order);
                order.OrderId = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrder - OrderDal: " + ex);
                return -1;
            }
        }
        public int UpdateOrder(Order order)
        {
            try
            {
                SqlParameter[] objParam_order = new SqlParameter[]
                    {
                     new SqlParameter("@OrderId", order.OrderId),
                    new SqlParameter("@OrderNo", order.OrderNo ?? (object)DBNull.Value),
                    new SqlParameter("@ServiceType", order.ServiceType ?? (object)DBNull.Value),
                    new SqlParameter("@CreateTime", order.CreateTime ?? (object)DBNull.Value),
                    new SqlParameter("@Amount", order.Amount ?? (object)DBNull.Value),
                    new SqlParameter("@PaymentStatus", order.PaymentStatus ?? (object)DBNull.Value),
                    new SqlParameter("@ClientId", order.ClientId ?? (object)DBNull.Value),
                    new SqlParameter("@ContactClientId", order.ContactClientId ?? (object)DBNull.Value),
                    new SqlParameter("@OrderStatus", order.OrderStatus ?? (object)DBNull.Value),
                    new SqlParameter("@ContractId", order.ContractId ?? (object)DBNull.Value),
                    new SqlParameter("@SmsContent", order.SmsContent ?? (object)DBNull.Value),
                    new SqlParameter("@PaymentType", order.PaymentType ?? (object)DBNull.Value),
                    new SqlParameter("@BankCode", order.BankCode ?? (object)DBNull.Value),
                    new SqlParameter("@PaymentDate", order.PaymentDate ?? (object)DBNull.Value),
                    new SqlParameter("@PaymentNo", order.PaymentNo ?? (object)DBNull.Value),
                    new SqlParameter("@ColorCode", order.ColorCode ?? (object)DBNull.Value),
                    new SqlParameter("@Discount", order.Discount ?? (object)DBNull.Value),
                    new SqlParameter("@Profit", order.Profit ?? (object)DBNull.Value),
                    new SqlParameter("@ExpriryDate", order.ExpriryDate ?? (object)DBNull.Value),
                    new SqlParameter("@StartDate", order.StartDate ?? (object)DBNull.Value),
                    new SqlParameter("@EndDate", order.EndDate ?? (object)DBNull.Value),
                    new SqlParameter("@ProductService", order.ProductService ?? (object)DBNull.Value),
                    new SqlParameter("@Note", order.Note ?? (object)DBNull.Value),
                    new SqlParameter("@UtmSource", order.UtmSource ?? (object)DBNull.Value),
                    new SqlParameter("@UpdateLast", order.UpdateLast ?? (object)DBNull.Value),
                    new SqlParameter("@SalerId", order.SalerId ?? (object)DBNull.Value),
                    new SqlParameter("@SalerGroupId", order.SalerGroupId ?? (object)DBNull.Value),
                    new SqlParameter("@UserUpdateId", order.UserUpdateId ?? (object)DBNull.Value),
                    new SqlParameter("@SystemType", order.SystemType ?? (object)DBNull.Value),
                    new SqlParameter("@AccountClientId", order.AccountClientId ?? (object)DBNull.Value),
                    new SqlParameter("@Description", order.Description ?? (object)DBNull.Value),
                    new SqlParameter("@BranchCode", order.BranchCode ?? (object)DBNull.Value),
                    new SqlParameter("@BookingInfo", order.BookingInfo ?? (object)DBNull.Value),
                    new SqlParameter("@Label", order.Label ?? (object)DBNull.Value),
                    new SqlParameter("@IsFinishPayment", order.IsFinishPayment ?? (object)DBNull.Value),
                    new SqlParameter("@PercentDecrease", order.PercentDecrease ?? (object)DBNull.Value),
                    new SqlParameter("@VoucherId", order.VoucherId ?? (object)DBNull.Value),
                    new SqlParameter("@Price", order.Price ?? (object)DBNull.Value),
                    new SqlParameter("@SupplierId", order.SupplierId ?? (object)DBNull.Value),
                    new SqlParameter("@DepartmentId", order.DepartmentId ?? (object)DBNull.Value),
                    new SqlParameter("@OperatorId", order.OperatorId ?? (object)DBNull.Value),
                    new SqlParameter("@UserVerify", order.UserVerify ?? (object)DBNull.Value),
                    new SqlParameter("@VerifyDate", order.VerifyDate ?? (object)DBNull.Value),
                    new SqlParameter("@DebtStatus", order.DebtStatus ?? (object)DBNull.Value),
                    new SqlParameter("@DebtNote", order.DebtNote ?? (object)DBNull.Value),
                    new SqlParameter("@Commission", order.Commission ?? (object)DBNull.Value),
                    new SqlParameter("@UtmMedium", order.UtmMedium ?? (object)DBNull.Value),
                    new SqlParameter("@Refund", order.Refund ?? (object)DBNull.Value),
                    new SqlParameter("@TotalFundCustomerCare", order.TotalFundCustomerCare ?? (object)DBNull.Value),
                    new SqlParameter("@AmountFinalize", order.AmountFinalize ?? (object)DBNull.Value),
                    new SqlParameter("@FinalizeDate", order.FinalizeDate ?? (object)DBNull.Value),
                    new SqlParameter("@IsLock", order.IsLock ?? (object)DBNull.Value),
                    new SqlParameter("@ClosingEndDate", order.ClosingEndDate ?? (object)DBNull.Value),
                    new SqlParameter("@TenantId", order.TenantId ?? (object)DBNull.Value)
                };

                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.UpdateOrder, objParam_order);
                order.OrderId = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrder - OrderDal: " + ex);
                return -1;
            }
        }
        public async Task<long> UpdateOrderSaler(long order_id, int user_commit)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var exists = _DbContext.Order.AsNoTracking().FirstOrDefault(s => s.OrderId == order_id);
                    if (exists != null && exists.OrderId > 0)
                    {
                        exists.UpdateLast = DateTime.Now;
                        exists.UserUpdateId = user_commit;
                        exists.SalerId = user_commit;
                        if (exists.OrderStatus == (int)OrderStatus.CREATED_ORDER)
                        {
                            exists.OrderStatus = (int)OrderStatus.CONFIRMED_SALE;
                        }
                        _DbContext.Order.Update(exists);
                        await _DbContext.SaveChangesAsync();
                    }
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderSaler - OrderDal: " + ex);
                return -2;
            }
        }
        public int IsClientAllowedToDebtNewService(double service_amount, long client_id, long order_id, int service_type)
        {
            try
            {
                SqlParameter[] objParam_order = new SqlParameter[4];
                objParam_order[0] = new SqlParameter("@ClientId", client_id);
                objParam_order[1] = new SqlParameter("@Amount", service_amount);
                objParam_order[2] = new SqlParameter("@OrderId", order_id);
                objParam_order[3] = new SqlParameter("@ServiceType", service_type);

                var table = _DbWorker.GetDataTable(StoreProcedureConstant.SP_CheckClientDebt, objParam_order);
                if (table != null && table.Rows.Count > 0)
                {
                    int id = -1;
                    id = (from row in table.AsEnumerable()
                          select new
                          {
                              IsPayable = Convert.ToInt32(row["IsPayable"])
                          }).First().IsPayable;
                    return id;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("IsClientAllowedToDebtNewService - OrderDal: " + ex);
                return -1;
            }
        }

        public int UpdateOrderOperator(long order_id)
        {
            try
            {
                SqlParameter[] objParam_order = new SqlParameter[1];
                objParam_order[0] = new SqlParameter("@Orderid", order_id);
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOperatorByOrderid, objParam_order);
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderOperator - OrderDal: " + ex);
                return -1;
            }
        }
        public async Task<long> UpdateOrderAmountFundCustomer(long order_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data = _DbContext.Order.AsNoTracking().FirstOrDefault(s => s.OrderId == order_id);
                    var list_tour_booking_all = await _DbContext.Tour.AsNoTracking().Where(s => s.OrderId == order_id).ToListAsync();
                    var list_tour_booking = list_tour_booking_all.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                    data.TotalFundCustomerCare = list_tour_booking != null && list_tour_booking.Count > 0 ? list_tour_booking.Sum(s => s.FundCustomerCare) : 0;
                    _DbContext.Order.Update(data);
                    await _DbContext.SaveChangesAsync();
                    return order_id;
                }


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderOperator - OrderDal: " + ex);
                return -1;
            }
        }
        public async Task<long> UpdateOrderFinishPayment(long OrderId, long Status)
        {
            try
            {
                SqlParameter[] objParam_updateFinishPayment = new SqlParameter[5];
                objParam_updateFinishPayment[0] = new SqlParameter("@OrderId", OrderId);
                objParam_updateFinishPayment[1] = new SqlParameter("@IsFinishPayment", DBNull.Value);
                objParam_updateFinishPayment[2] = new SqlParameter("@PaymentStatus", DBNull.Value);
                objParam_updateFinishPayment[3] = new SqlParameter("@Status", Status);
                objParam_updateFinishPayment[4] = new SqlParameter("@DebtStatus", DBNull.Value);
                var data = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderFinishPayment, objParam_updateFinishPayment);
                return data;


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderFinishPayment - OrderDal: " + ex);
                return -1;
            }
        }
        public async Task<long> UpdateServiceStatusByOrderId(long OrderId, long StatusFilter, long Status)
        {
            try
            {
                SqlParameter[] objParam_updateFinishPayment = new SqlParameter[3];
                objParam_updateFinishPayment[0] = new SqlParameter("@OrderId", OrderId);
                objParam_updateFinishPayment[1] = new SqlParameter("@StatusFilter", StatusFilter);
                objParam_updateFinishPayment[2] = new SqlParameter("@Status", Status);
                var data = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateServiceStatusByOrderId, objParam_updateFinishPayment);
                return data;


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateServiceStatusByOrderId - OrderDal: " + ex);
                return -1;
            }
        }
        public async Task<long> UpdateAllServiceStatusByOrderId(long OrderId, long Status)
        {
            try
            {
                SqlParameter[] objParam_updateFinishPayment = new SqlParameter[2];
                objParam_updateFinishPayment[0] = new SqlParameter("@OrderId", OrderId);
                objParam_updateFinishPayment[1] = new SqlParameter("@Status", Status);

                var data = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateAllServiceStatusByOrderId, objParam_updateFinishPayment);
                return data;


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateServiceStatusByOrderId - OrderDal: " + ex);
                return -1;
            }
        }
        public async Task<long> RePushDeclineServiceToOperator(long OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                var data = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateServiceStatusFromDecline, objParam);
                return data;


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateServiceStatusByOrderId - OrderDal: " + ex);
                return -1;
            }
        }
        public async Task<DataTable> GetAllContractPayByOrderID(long OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAllServiceByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllServiceByOrderId - OrderDal: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetContractPayByOrderId(long OrderId)
        {
            try
            {

                SqlParameter[] objParam_contractPay = new SqlParameter[1];
                objParam_contractPay[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetContractPayByOrderId, objParam_contractPay);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayByOrderId - ContractPayDAL. " + ex);
                return null;
            }
        }
        public List<long> GetAllOrderIDs()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.Order.AsNoTracking().Select(x => x.OrderId).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllOrderIDs - OrderDal: " + ex);
                return new List<long>();
            }
        }
        public async Task<TotalCustomerCareFundViewModel> GetTotalCustomerCareFund(string SalerIds, long ClientId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@SalerIds ", SalerIds);
                objParam[1] = ClientId != null && ClientId > 0 ? new SqlParameter("@ClientId ", ClientId) : new SqlParameter("@ClientId ", DBNull.Value);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetTotalCustomerCareFund, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<TotalCustomerCareFundViewModel>();
                    return data[0];
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalCustomerCareFund - OrderDal. " + ex);
            }
            return null;
        }
        public async Task<long> OrderBookClosing(OrderBookClosingViewModel model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@FromDate ", ((DateTime)model.FromDate).ToString("yyyy/MM/dd hh:mm:ss") + " AM");
                objParam[1] = new SqlParameter("@ToDate ", model.ToDate.ToString("yyyy/MM/dd hh:mm:ss") + " PM");
                objParam[2] = new SqlParameter("@UserFinalize ", model.UserFinalize);
                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_OrderBookClosing, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {

                    var TotalRow = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"]);

                    return TotalRow;
                }
                return -1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OrderBookClosing - OrderDal. " + ex);
            }
            return -1;
        }
        public async Task<long> CheckBookClosingByDate(DateTime FromDate, DateTime ToDate)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@FromDate ", FromDate);
                objParam[1] = new SqlParameter("@ToDate ", ToDate);
                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.sp_CheckBookClosingByDate, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {

                    var RESULT = dt.Rows[0]["RESULT"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["RESULT"]);

                    return RESULT;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckBookClosingByDate - OrderDal. " + ex);
            }
            return 0;
        }
        public async Task<long> UpdateBookClosingByOrderId(long OrderId, long IsLock, long UpdateBy)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                objParam[1] = new SqlParameter("@IsLock", IsLock);
                objParam[2] = new SqlParameter("@UpdateBy", UpdateBy);
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateBookClosingByOrderId, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckBookClosingByDate - OrderDal. " + ex);
            }
            return 0;
        }
        public async Task<DataTable> GetListOrderBookClosingByOrderId(long OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListOrderBookClosingByOrderId, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckBookClosingByDate - OrderDal. " + ex);
            }
            return null;
        }
        public async Task<DataTable> CheckAmountRemainBySalerId(long SalerId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@SalerId", SalerId);
                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_CheckAmountRemainBySalerId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckAmountRemainBySalerId - OrderDal: " + ex);
            }
            return null;
        }
        public async Task<int> UpdateOrderIsSalerDebtLimit(long OrderId, long Status, long UpdatedBy, long UserVerify)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                objParam[1] = new SqlParameter("@IsSalerDebtLimit", Status);
                objParam[2] = new SqlParameter("@UpdatedBy", UpdatedBy);
                objParam[3] = UserVerify == 0 ? new SqlParameter("@UserVerify", DBNull.Value) : new SqlParameter("@UserVerify", UserVerify);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderIsSalerDebtLimit, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderServiceByOrderId - OrderDal: " + ex);
            }
            return 0;
        }
    }
}
