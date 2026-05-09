using Entities.Models;
using Entities.ViewModels.FlightWarehouse;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    public class FlightWarehouseController : Controller
    {
        private readonly IFlightWarehouseRepository _flightWarehouseRepository;
        private readonly ManagementUser _managementUser;

        public FlightWarehouseController(IFlightWarehouseRepository flightWarehouseRepository, ManagementUser managementUser)
        {
            _flightWarehouseRepository = flightWarehouseRepository;
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
                var dt = await _flightWarehouseRepository.GetListFlightWarehouse(searchModel, pageIndex, pageSize);
                ViewBag.PageIndex = pageIndex;
                ViewBag.PageSize = pageSize;
                return PartialView(dt);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - FlightWarehouseController: " + ex);
            }
            return PartialView(null);
        }

        public async Task<IActionResult> Upsert(long id = 0)
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
            }

            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertBooking([FromBody] FlightWarehouseUpsertModel model)
        {
            try
            {
                if (model == null || model.Booking == null)
                {
                    return Json(new { status = 1, msg = "Dữ liệu không hợp lệ" });
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
    }
}
