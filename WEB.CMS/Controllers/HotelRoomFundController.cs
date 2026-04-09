using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.Adavigo.CMS.Controllers
{
    [CustomAuthorize]
    public class HotelRoomFundController : Controller
    {
        private readonly IHotelRoomFundRepository _hotelRoomFundRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly ManagementUser _managementUser;

        public HotelRoomFundController(IHotelRoomFundRepository hotelRoomFundRepository,
            IHotelRepository hotelRepository,
            ISupplierRepository supplierRepository,
            ManagementUser managementUser)
        {
            _hotelRoomFundRepository = hotelRoomFundRepository;
            _hotelRepository = hotelRepository;
            _supplierRepository = supplierRepository;
            _managementUser = managementUser;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(HotelRoomFundSearchMode searchModel)
        {
            var model = new GenericViewModel<HotelRoomFundModel>();
            try
            {
                var datas = await _hotelRoomFundRepository.GetListHotelRoomFund(searchModel);
                model.CurrentPage = searchModel.PageIndex;
                model.ListData = datas;
                model.PageSize = searchModel.PageSize;
                model.TotalRecord = datas != null && datas.Any() ? datas.First().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.PageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - HotelRoomFundController: " + ex);
            }
            return PartialView(model);
        }

        public async Task<IActionResult> AddOrUpdate(int id)
        {
            var model = new HotelRoomFundModel();
            if (id > 0)
            {
                model = await _hotelRoomFundRepository.GetById(id);
                if (model != null)
                {
                    model.HotelRoomFundDetails = await _hotelRoomFundRepository.GetListHotelRoomFundDetail2(id);
                }
            }
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(HotelRoomFund model, List<HotelRoomFundDetail> details)
        {
            try
            {
                int userId = _managementUser.GetCurrentUser().Id;
                if (model.Id > 0)
                {
                    model.UpdateBy = userId.ToString();
                    model.UpdateDate = DateTime.Now;
                    await _hotelRoomFundRepository.UpdateHotelRoomFund(model);
                    // Xóa các chi tiết cũ trước khi nạp lại
                    if (details != null && details.Count > 0)
                        await _hotelRoomFundRepository.DeleteHotelRoomFundDetailByFundId(model.Id, details[0].HotelRoomId);
                }
                else
                {
                    model.CreateBy = userId.ToString();
                    model.CreateDate = DateTime.Now;
                    model.Id = await _hotelRoomFundRepository.InsertHotelRoomFund(model);
                }

                if (model.Id > 0 && details != null && details.Any())
                {
                    foreach (var detail in details)
                    {
                        detail.HotelRoomFundId = model.Id;
                        await _hotelRoomFundRepository.InsertHotelRoomFundDetail(detail);
                    }
                }

                return Json(new { status = 0, msg = "Thành công" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Save - HotelRoomFundController: " + ex);
                return Json(new { status = 1, msg = "Thất bại" });
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            var viewModel = new HotelRoomFundDetailViewModel();
            try
            {
                var model = await _hotelRoomFundRepository.GetById(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }

                viewModel.Id = model.Id;
                viewModel.SupplierName = model.SupplierName;
                viewModel.HotelName = model.HotelName;
                viewModel.TotalRooms = model.TotalRooms;

                // Lấy danh sách chi tiết quỹ phòng
                var details = await _hotelRoomFundRepository.GetListHotelRoomFundDetail(id);



                // Thiết lập 7 ngày bắt đầu từ hôm nay
                var startDate = DateTime.Today;
                for (int i = 0; i < 7; i++)
                {
                    viewModel.DisplayDates.Add(startDate.AddDays(i));
                }

                // Nhóm details theo HotelRoomId
                if (details != null && details.Any())
                {
                    // Lấy danh sách mỗi Hạng phòng 1 bản ghi đại diện (unique categories)
                    var uniqueCategories = details.GroupBy(d => d.HotelRoomId).Select(i => i.First()).ToList();
                    
                    foreach (var category in uniqueCategories)
                    {
                        // Lấy tất cả các chi tiết (dải ngày) thuộc về hạng phòng này
                        var categoryDetails = details.Where(d => d.HotelRoomId == category.HotelRoomId).ToList();

                        var row = new RoomCategoryRow
                        {
                            HotelRoomId = category.HotelRoomId,
                            RoomName = category.RoomName,
                            TotalCapacity = categoryDetails.Sum(s => s.TotalRoomNights)
                        };

                        // Tính số phòng đã phân bổ và đã đặt cho mỗi ngày dựa trên danh sách chi tiết của hạng phòng này
                        foreach (var date in viewModel.DisplayDates)
                        {
                            decimal allocated = 0;
                            decimal booked = 0;
                            foreach (var detail in categoryDetails)
                            {
                                if (date >= detail.StartDate.Date && date <= detail.EndDate.Date)
                                {
                                    allocated += detail.NumberOfRooms;
                                    booked += detail.TotalBookedRooms;
                                }
                            }
                            row.DailyAllocations[date] = allocated;
                            row.DailyBooked[date] = booked;
                        }

                        viewModel.RoomCategories.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Detail - HotelRoomFundController: " + ex);
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetDetailData(int id, string startDateStr, string endDateStr)
        {
            try
            {
                var model = await _hotelRoomFundRepository.GetById(id);
                if (model == null)
                    return Json(new { status = 1, msg = "Không tìm thấy dữ liệu" });

                var details = await _hotelRoomFundRepository.GetListHotelRoomFundDetail(id);

                DateTime startDate, endDate;
                if (!string.IsNullOrEmpty(startDateStr) && !string.IsNullOrEmpty(endDateStr))
                {
                    startDate = DateTime.ParseExact(startDateStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    endDate = DateTime.ParseExact(endDateStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    startDate = DateTime.Today;
                    endDate = startDate.AddDays(6);
                }

                // Giới hạn hiển thị tối đa 45 ngày để đảm bảo hiệu năng
                if ((endDate - startDate).TotalDays > 45)
                {
                    endDate = startDate.AddDays(45);
                }

                var displayDates = new List<DateTime>();
                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    displayDates.Add(d);
                }

                var categories = new List<object>();
                if (details != null && details.Any())
                {
                    // Lấy danh sách mỗi Hạng phòng 1 bản ghi đại diện (unique categories)
                    var uniqueCategories = details.GroupBy(d => d.HotelRoomId).Select(i => i.First()).ToList();

                    foreach (var category in uniqueCategories)
                    {
                        // Lấy tất cả các chi tiết (dải ngày) thuộc về hạng phòng này
                        var categoryDetails = details.Where(d => d.HotelRoomId == category.HotelRoomId).ToList();

                        var dailyData = new List<object>();
                        foreach (var date in displayDates)
                        {
                            decimal allocated = 0;
                            decimal booked = 0;
                            foreach (var detail in categoryDetails)
                            {
                                if (date >= detail.StartDate.Date && date <= detail.EndDate.Date)
                                {
                                    allocated += detail.NumberOfRooms;
                                    booked += detail.TotalBookedRooms;
                                }
                            }
                            dailyData.Add(new
                            {
                                date = date.ToString("dd/MM/yyyy"),
                                dayOfWeek = date.ToString("ddd", new System.Globalization.CultureInfo("vi-VN")).ToUpper(),
                                day = date.Day,
                                isToday = date.Date == DateTime.Today,
                                allocated = allocated,
                                booked = booked,
                                percentage = allocated > 0 ? Math.Round((booked / allocated) * 100, 0) : 0
                            });
                        }
                        categories.Add(new
                        {
                            hotelRoomId = category.HotelRoomId,
                            roomName = category.RoomName,
                            totalCapacity = categoryDetails.Sum(s => s.TotalRoomNights),
                            dailyData = dailyData
                        });
                    }
                }

                var dates = displayDates.Select(d => new
                {
                    date = d.ToString("dd/MM/yyyy"),
                    dayOfWeek = d.ToString("ddd", new System.Globalization.CultureInfo("vi-VN")).ToUpper(),
                    day = d.Day,
                    isToday = d.Date == DateTime.Today
                }).ToList();

                return Json(new { status = 0, dates = dates, categories = categories });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailData - HotelRoomFundController: " + ex);
                return Json(new { status = 1, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetHotelBySupplier(int supplierId)
        {
            try
            {
                var hotels = _supplierRepository.GetHotelListBySuplierId(supplierId);
                return Json(new { status = 0, data = hotels });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetRoomTypeByHotel(int hotelId)
        {
            try
            {
                var roomTypes = _hotelRepository.GetHotelRoomList(hotelId, 1, 100);
                return Json(new { status = 0, data = roomTypes });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckExistence(int hotelId, int supplierId)
        {
            try
            {
                var exists = await _hotelRoomFundRepository.GetByHotelAndSupplier(hotelId, supplierId);
                if (exists != null)
                {
                    return Json(new { status = 1, msg = "Quỹ phòng cho khách sạn và nhà cung cấp này đã tồn tại trong hệ thống." });
                }
                return Json(new { status = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, msg = ex.Message });
            }
        }
    }
}
