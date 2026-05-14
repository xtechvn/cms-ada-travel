using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.FlightWarehouse;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Repositories.IRepositories;
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
                var phantram = ((model.Booking.AgencyTotalTicket==null?0: (double)model.Booking.AgencyTotalTicket) / (model.Booking.TotalTicket==null?0: (double)model.Booking.TotalTicket) * 100).ToString("0.00");
                ViewBag.phantram = phantram;
                if (flightDate <= DateTime.Now)
                {
                    ViewBag.Btnadd = 1;
                }
            }

            return View(model);
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
            if(flightDate<=DateTime.Now)
            {
                ViewBag.add = 1;
            }
            DateTime holdLimit = DateTime.Now;
            if (flightDate.HasValue)
            {
                if (flightDate.Value.Date > DateTime.Now.Date)
                {
                    holdLimit = DateTime.Now.AddHours(1);
                }
                else if (DateTime.Now.Date < flightDate.Value.Date)
                {
                    holdLimit = DateTime.Now.AddHours(124);
                }

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
        public async Task<IActionResult> UpsertHoldTicket([FromBody]FlightWarehouseHoldTicketModel model)
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
                model.ExpirationDate =  DateUtil.StringToDateTime(model.ExpirationDateStr);
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
    }
}
