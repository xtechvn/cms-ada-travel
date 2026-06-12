using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.FlightWarehouse;
using Microsoft.AspNetCore.Mvc;
using Nest;
using OfficeOpenXml;
using Repositories.IRepositories;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    public class FlightWarehouseController : Controller
    {
        private readonly IFlightWarehouseRepository _flightWarehouseRepository;
        private readonly IFlyBookingDetailRepository _flyBookingDetailRepository;
        private readonly IAirlinesRepository _airlinesRepository;
        private readonly IUserRepository _userRepository;
        private readonly ManagementUser _managementUser;

        public FlightWarehouseController(IFlightWarehouseRepository flightWarehouseRepository,
            IFlyBookingDetailRepository flyBookingDetailRepository,
            IAirlinesRepository airlinesRepository,
            IUserRepository userRepository,
            ManagementUser managementUser)
        {
            _flightWarehouseRepository = flightWarehouseRepository;
            _flyBookingDetailRepository = flyBookingDetailRepository;
            _airlinesRepository = airlinesRepository;
            _userRepository = userRepository;
            _managementUser = managementUser;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(GetListFlightWarehouseModel searchModel, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var data = await _flightWarehouseRepository.GetListFlightWarehouse(searchModel, pageIndex, pageSize);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - FlightWarehouseController: " + ex);
            }
            return PartialView(null);
        }

        public async Task<IActionResult> Upsert(long id = 0, long order_id = 0, string group_fly = "")
        {
            var model = new FlightWarehouseUpsertModel
            {
                Booking = new FlightWarehouseBookingModel(),
                Segments = new List<FlightWarehouseSegmentModel>(),
                Prices = new List<FlightWarehousePriceModel>()
            };

            ViewBag.flybooking_from = null;
            ViewBag.flybooking_to = null;

            if (id > 0)
            {
                model.Booking = await _flightWarehouseRepository.GetBookingById(id);
                model.Segments = await _flightWarehouseRepository.GetSegmentsByBookingId(id);
                model.Prices = await _flightWarehouseRepository.GetPricesByBookingId(id);
                ViewBag.flybooking_from = await _airlinesRepository.GetAirportByCode(model.Booking.DeparturePoint);
                ViewBag.flybooking_to = await _airlinesRepository.GetAirportByCode(model.Booking.ArrivalPoint);
                ViewBag.airline = await _airlinesRepository.getAllAirlines();
            }

            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertBooking([FromBody] FlightWarehouseUpsertModel model)
        {
            try
            {
                var _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.Name) == null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (model == null || model.Booking == null)
                {
                    return Json(new { status = 1, msg = "Dữ liệu không hợp lệ" });
                }
                if (model.Booking.Id > 0)
                {
                    model.Booking.UpdatedBy = _UserId;
                }
                else
                {
                    model.Booking.CreatedBy = _UserId;

                }
                // Save Booking
                var bookingId = await _flightWarehouseRepository.UpsertBooking(model.Booking);
                if (bookingId <= 0)
                {
                    return Json(new { status = 1, msg = "Lưu thông tin booking thất bại" });
                }

                // Save Segments
                if (model.Segments != null)
                {
                    foreach (var segment in model.Segments)
                    {
                        segment.BookingId = bookingId;
                        segment.FlightDate = DateUtil.StringToDateTime(segment.FlightDateStr);
                        await _flightWarehouseRepository.UpsertSegment(segment);
                    }
                }

                // Save Prices
                if (model.Prices != null)
                {
                    foreach (var price in model.Prices)
                    {
                        price.BookingId = bookingId;
                        await _flightWarehouseRepository.UpsertPrice(price);
                    }
                }

                return Json(new { status = 0, msg = "Thành công", data = bookingId });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertBooking - FlightWarehouseController: " + ex);
                return Json(new { status = 1, msg = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AirPortCodeSuggestion(string txt_search)
        {
            try
            {
                var data = await _airlinesRepository.GetAirportCode(txt_search);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = data
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AirPortCodeSuggestion - FlightWarehouseController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<AirPortCode>()
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AirlinesSuggestion(string txt_search)
        {

            try
            {
                var data = await _airlinesRepository.SearchAirlines(txt_search);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = data
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("HotelSuggestion - FlightWarehouseController: " + ex.ToString());
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<CustomerESViewModel>()
                });
            }

        }

        public async Task<IActionResult> Detail(long id)
        {
            var model = new FlightWarehouseUpsertModel
            {
                Booking = new FlightWarehouseBookingModel(),
                Segments = new List<FlightWarehouseSegmentModel>(),
                Prices = new List<FlightWarehousePriceModel>()
            };

            if (id > 0)
            {
                model.Booking = await _flightWarehouseRepository.GetBookingById(id);
                model.Segments = await _flightWarehouseRepository.GetSegmentsByBookingId(id);
                model.Prices = await _flightWarehouseRepository.GetPricesByBookingId(id);
                ViewBag.flybooking_from = await _airlinesRepository.GetAirportByCode(model.Booking.DeparturePoint);
                ViewBag.flybooking_to = await _airlinesRepository.GetAirportByCode(model.Booking.ArrivalPoint);
                ViewBag.airline = await _airlinesRepository.getAllAirlines();
                var flightDate = model.Segments.FirstOrDefault(x => x.SegmentType == 0)?.FlightDate;
                ViewBag.Btnadd = 0;
                var AgencyTotalTicket = model.Booking.AgencyTotalTicket > 0 ? ((model.Booking.AgencyTotalTicket == null ? 0 : (double)model.Booking.AgencyTotalTicket) + model.Booking.RemainTicket > 0 ? (double)model.Booking.RemainTicket : 0) : 0;
                var TotalTicket = model.Booking.TotalTicket > 0 ? (double)model.Booking.TotalTicket : 0;
                var TotalClosedTicket = model.Booking.TotalClosedTicket > 0 ? (double)model.Booking.TotalClosedTicket : 0;
                var phantram = TotalTicket > 0 ? ((AgencyTotalTicket + TotalClosedTicket) / TotalTicket * 100) : 0;
                ViewBag.phantram = phantram.ToString("0.00");
                if (flightDate <= DateTime.Now)
                {
                    ViewBag.Btnadd = 1;
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetBookingSummary(long bookingId)
        {
            try
            {
                var booking = await _flightWarehouseRepository.GetBookingById(bookingId);
                if (booking == null) return Json(new { status = 1, msg = "Không tìm thấy thông tin" });
                var AgencyTotalTicket = booking.AgencyTotalTicket > 0 ? ((booking.AgencyTotalTicket == null ? 0 : (double)booking.AgencyTotalTicket) + booking.RemainTicket > 0 ? (double)booking.RemainTicket : 0) : 0;
                var RemainTicket = booking.RemainTicket > 0 ? (double)booking.RemainTicket : 0;
                var TotalTicket = booking.TotalTicket > 0 ? (double)booking.TotalTicket : 0;
                var TotalClosedTicket = booking.TotalClosedTicket > 0 ? (double)booking.TotalClosedTicket : 0;
                var phantram = TotalTicket > 0 ? ((AgencyTotalTicket + TotalClosedTicket) / TotalTicket * 100) : 0;
                return Json(new
                {
                    status = 0,
                    data = new
                    {
                        total = booking.TotalTicket,
                        closed = booking.TotalClosedTicket ?? 0,
                        ada = booking.AdaTotalTicket ?? 0,
                        agency = booking.AgencyTotalTicket ?? 0,
                        remaining = booking.RemainTicket ?? 0,
                        percent = phantram.ToString("0.00")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> HoldTicketList(FlightWarehouseHoldTicketSearchModel searchModel)
        {
            try
            {
                var data = await _flightWarehouseRepository.GetListHoldTicket(searchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("HoldTicketList - FlightWarehouseController: " + ex);
            }
            return PartialView(null);
        }

        public async Task<IActionResult> HoldTicketPopup(long bookingId)
        {
            ViewBag.bookingId = bookingId;
            var prices = await _flightWarehouseRepository.GetPricesByBookingId(bookingId);
            var segments = await _flightWarehouseRepository.GetSegmentsByBookingId(bookingId);
            var flightDate = segments.FirstOrDefault(x => x.SegmentType == 0)?.FlightDate;
            ViewBag.add = 0;
            if (flightDate <= DateTime.Now)
            {
                ViewBag.add = 1;
            }
            DateTime holdLimit = DateTime.Now;
            if (flightDate.HasValue)
            {
                // 🔥 Nếu flightDate còn hơn 7 ngày
                if (flightDate.Value.AddDays(-7) > DateTime.Now)
                {
                    holdLimit = DateTime.Now.AddHours(24);
                }
                else
                {
                    // 🔥 Còn dưới 7 ngày
                    holdLimit = DateTime.Now.AddHours(1);
                }

                // 🔥 Không được vượt quá giờ bay
                if (holdLimit > flightDate.Value)
                {
                    holdLimit = flightDate.Value;
                }
            }
            ViewBag.HoldLimit = holdLimit;

            var users = _userRepository.GetAll();
            ViewBag.Users = users;

            return PartialView(prices);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertHoldTicket([FromBody] FlightWarehouseHoldTicketModel model)
        {
            try
            {
                var _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                if (model == null || model.FlightWarehouseBookingId <= 0)
                {
                    return Json(new { status = 1, msg = "Dữ liệu không hợp lệ" });
                }
                model.ExpirationDate = DateUtil.StringToDateTime(model.ExpirationDateStr);
                model.CreatedBy = _UserId;
                model.CreatedDate = DateTime.Now;

                var result = await _flightWarehouseRepository.UpsertHoldTicket(model);
                if (result > 0)
                {
                    return Json(new { status = 0, msg = "Giữ vé thành công" });
                }
                else
                {
                    return Json(new { status = 1, msg = "Giữ vé thất bại" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertHoldTicket - FlightWarehouseController: " + ex);
                return Json(new { status = 1, msg = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public async Task<IActionResult> AgencyHoldPopup(long bookingId)
        {
            var booking = await _flightWarehouseRepository.GetBookingById(bookingId);
            return PartialView(booking);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAgencyHold(long bookingId, int quantity)
        {
            try
            {
                var booking = await _flightWarehouseRepository.GetBookingById(bookingId);
                if (booking == null) return Json(new { status = 1, msg = "Không tìm thấy thông tin đơn hàng" });

                booking.AgencyTotalTicket = quantity;
                var result = await _flightWarehouseRepository.UpsertBooking(booking);

                if (result > 0) return Json(new { status = 0, msg = "Cập nhật thành công" });
                else return Json(new { status = 1, msg = "Cập nhật thất bại" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAgencyHold - FlightWarehouseController: " + ex);
                return Json(new { status = 1, msg = "Lỗi hệ thống" });
            }
        }
        public IActionResult ImportWSExcel()
        {

            return PartialView();

        }
        [HttpPost]
        public async Task<IActionResult> ImportWSExcelListing(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { status = 1, msg = "Vui lòng chọn file." });
                }

                var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.FirstOrDefault();
                    if (ws == null) return Json(new { status = 1, msg = "File không có dữ liệu." });

                    var endRow = ws.Cells.End.Row;
                    var startRow = 4;
                    await _flightWarehouseRepository.DeleteFlightWarehouseBookingEX();

                    var _UserId = 0;
                    if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                    {
                        _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    }

                    for (int row = startRow; row <= endRow; row++)
                    {
                        var cellRange = ws.Cells[row, 1, row, 16];
                        var isRowEmpty = cellRange.All(c => c.Value == null);
                        if (isRowEmpty) break;

                        var Hanh_trinh = ws.Cells[row, 1].Value?.ToString();
                        var Hang_bay = ws.Cells[row, 2].Value?.ToString();
                        var Ma_dat_cho = ws.Cells[row, 3].Value?.ToString();
                        var Thoi_gian = ws.Cells[row, 4].Value?.ToString();
                        var Go_time = ws.Cells[row, 5].Value?.ToString();
                        var Go_shcb = ws.Cells[row, 6].Value?.ToString();
                        var Back_time = ws.Cells[row, 7].Value?.ToString();
                        var Back_shcb = ws.Cells[row, 8].Value?.ToString();
                        var HL_xachtay = ws.Cells[row, 9].Value?.ToString();
                        var HL_kygui = ws.Cells[row, 10].Value?.ToString();

                        var Gia_KL_str = ws.Cells[row, 13].Value?.ToString().Replace(".","").Replace(",","");
                        var Gia_DL_str = ws.Cells[row, 14].Value?.ToString().Replace(".","").Replace(".", "");
                        var SL_str = ws.Cells[row, 15].Value?.ToString();
                        var DLkhac_ban_str = ws.Cells[row, 16].Value?.ToString();

                        var Gia_KL = string.IsNullOrEmpty(Gia_KL_str) ? 0m : Convert.ToDecimal(Gia_KL_str);
                        var Gia_DL = string.IsNullOrEmpty(Gia_DL_str) ? 0m : Convert.ToDecimal(Gia_DL_str);
                        var SL = string.IsNullOrEmpty(SL_str) ? 0 : Convert.ToInt32(SL_str);
                        var DLkhac_ban = string.IsNullOrEmpty(DLkhac_ban_str) ? 0 : Convert.ToInt32(DLkhac_ban_str);

                        var list_Hanh_trinh = Hanh_trinh != null ? Hanh_trinh.Split('-') : new string[] { "", "" };
                        var list_Thoi_gian = Thoi_gian != null ? Thoi_gian.Split('-') : new string[] { "", "" };

                        var model = new FlightWarehouseUpsertModel
                        {
                            Booking = new FlightWarehouseBookingModel(),
                            Segments = new List<FlightWarehouseSegmentModel>(),
                            Prices = new List<FlightWarehousePriceModel>()
                        };

                        model.Booking.AgencyTotalTicket = DLkhac_ban;
                        model.Booking.TotalTicket = SL;
                        model.Booking.Note = Hanh_trinh;
                        model.Booking.DeparturePoint = list_Hanh_trinh.Length > 0 ? list_Hanh_trinh[0].Trim() : "";
                        model.Booking.ArrivalPoint = list_Hanh_trinh.Length > 1 ? list_Hanh_trinh[1].Trim() : "";
                        model.Booking.CarryOnBaggage = HL_xachtay;
                        model.Booking.CheckedBaggage = HL_kygui;
                        model.Booking.IsRefundable = 0;
                        model.Booking.FundType = 1;
                        model.Booking.TripType = string.IsNullOrEmpty(Back_time) ? 1 : 2;
                        model.Booking.CreatedBy = _UserId;

                        var model_price_1 = new FlightWarehousePriceModel();
                        model_price_1.PriceType = 1;
                        model_price_1.ChdPrice = model_price_1.AdtPrice = Gia_DL - 50000;
                        model_price_1.ChdAmount = model_price_1.AdtAmount = Gia_DL;
                        model_price_1.ChdProfit = model_price_1.AdtProfit = 50000;
                        model_price_1.InfPrice = (Gia_DL * 10 / 100) - 50000;
                        model_price_1.InfAmount = Gia_DL * 10 / 100;
                        model_price_1.InfProfit = 50000;
                        model.Prices.Add(model_price_1);

                        var model_price_2 = new FlightWarehousePriceModel();
                        model_price_2.PriceType = 2;
                        model_price_2.ChdPrice = model_price_2.AdtPrice = Gia_KL - 50000;
                        model_price_2.ChdAmount = model_price_2.AdtAmount = Gia_KL;
                        model_price_2.ChdProfit = model_price_2.AdtProfit = 50000;
                        model_price_2.InfPrice = (Gia_KL * 10 / 100) - 50000;
                        model_price_2.InfAmount = Gia_KL * 10 / 100;
                        model_price_2.InfProfit = 50000;
                        model.Prices.Add(model_price_2);

                        var model_Go = new FlightWarehouseSegmentModel();
                        model_Go.SegmentType = 0;
                        var goDateStr = (list_Thoi_gian.Length > 0 ? list_Thoi_gian[0].Trim() : "") + " " +Convert.ToDateTime(Go_time).ToString("HH:mm:ss");
                        model_Go.FlightDateStr = goDateStr;
                        model_Go.FlightDate = DateUtil.StringToDateTime(goDateStr);
                        model_Go.Airline = Hang_bay;
                        model_Go.FlightCode = Go_shcb;
                        model_Go.Pnrcode = Ma_dat_cho;
                        model.Segments.Add(model_Go);

                        if (model.Booking.TripType == 2 && !string.IsNullOrEmpty(Back_time))
                        {
                            var model_Back = new FlightWarehouseSegmentModel();
                            model_Back.SegmentType = 1;
                            var backDateStr = (list_Thoi_gian.Length > 1 ? list_Thoi_gian[1].Trim() : "") + " " + Convert.ToDateTime(Back_time).ToString("HH:mm:ss");
                            model_Back.FlightDateStr = backDateStr;
                            model_Back.FlightDate = DateUtil.StringToDateTime(backDateStr);
                            model_Back.Airline = Hang_bay;
                            model_Back.FlightCode = Back_shcb;
                            model_Back.Pnrcode = Ma_dat_cho;
                            model.Segments.Add(model_Back);
                        }

                        // Save Booking
                        var bookingId = await _flightWarehouseRepository.UpsertBooking(model.Booking);
                        if (bookingId <= 0)
                        {
                            return Json(new { status = 1, msg = "Lưu thông tin booking thất bại ở dòng " + row });
                        }

                        // Save Segments
                        if (model.Segments != null)
                        {
                            foreach (var segment in model.Segments)
                            {
                                segment.BookingId = bookingId;
                                await _flightWarehouseRepository.UpsertSegment(segment);
                            }
                        }

                        // Save Prices
                        if (model.Prices != null)
                        {
                            foreach (var price in model.Prices)
                            {
                                price.BookingId = bookingId;
                                await _flightWarehouseRepository.UpsertPrice(price);
                            }
                        }
                    }
                }

                return Json(new { status = 0, msg = "Tải lên thành công" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ImportWSExcelListing - FlightWarehouseController: " + ex.ToString());
                return Json(new { status = 1, msg = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
