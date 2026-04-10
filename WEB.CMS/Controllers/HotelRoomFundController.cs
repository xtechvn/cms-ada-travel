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
                viewModel.HotelId = model.HotelId;
                viewModel.SupplierId = model.SupplierId ?? 0;
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

                if (details != null && details.Any())
                {
                    // Lấy danh sách mỗi Hạng phòng 1 bản ghi đại diện (unique categories)
                    var uniqueCategories = details.GroupBy(d => d.HotelRoomId).Select(i => i.First()).ToList();
                    
                    foreach (var category in uniqueCategories)
                    {
                        var row = new RoomCategoryRow
                        {
                            HotelRoomId = category.HotelRoomId,
                            RoomName = category.RoomName,
                            TotalCapacity = details.Where(d => d.HotelRoomId == category.HotelRoomId).Sum(s => s.TotalRoomNights)
                        };
                        viewModel.RoomCategories.Add(row);
                    }

                    // Truy vấn lại theo từng đêm để lấy số phòng đã đặt chính xác cho đêm đó
                    foreach (var date in viewModel.DisplayDates)
                    {
                        var nightDetails = await _hotelRoomFundRepository.GetListHotelRoomFundDetail(id, date, date.AddDays(1));
                        
                        foreach (var row in viewModel.RoomCategories)
                        {
                            var categoryNights = nightDetails?.Where(d => d.HotelRoomId == row.HotelRoomId).ToList();
                            if (categoryNights != null && categoryNights.Any())
                            {
                                // Cộng dồn số phòng phân bổ từ tất cả các bản ghi quỹ của hạng phòng này trong đêm đó
                                row.DailyAllocations[date] = categoryNights[0].NumberOfRooms;
                                // Số phòng đã đặt là giá trị gộp của cả hạng phòng nên lấy giá trị lớn nhất (hoặc đầu tiên)
                                row.DailyBooked[date] = categoryNights[0].TotalBookedRooms;
                            }
                            else
                            {
                                row.DailyAllocations[date] = 0;
                                row.DailyBooked[date] = 0;
                            }
                        }
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

                    // Khởi tạo danh sách kết quả cho từng hạng phòng
                    var rowList = uniqueCategories.Select(c => new
                    {
                        hotelRoomId = c.HotelRoomId,
                        roomName = c.RoomName,
                        totalCapacity = details.Where(d => d.HotelRoomId == c.HotelRoomId).Sum(s => s.TotalRoomNights),
                        dailyList = new List<object>()
                    }).ToList();

                    // Truy vấn theo từng đêm (để lấy booked chính xác)
                    foreach (var date in displayDates)
                    {
                        var nightDetails = await _hotelRoomFundRepository.GetListHotelRoomFundDetail(id, date, date.AddDays(1));
                        
                        foreach (var row in rowList)
                        {
                            var categoryNights = nightDetails?.Where(d => d.HotelRoomId == row.hotelRoomId).ToList();
                            decimal allocated = 0;
                            decimal booked = 0;

                            if (categoryNights != null && categoryNights.Any())
                            {
                                allocated = categoryNights[0].NumberOfRooms;
                                booked = categoryNights[0].TotalBookedRooms;
                            }

                            row.dailyList.Add(new
                            {
                                date = date.ToString("dd/MM/yyyy"),
                                day = date.Day,
                                dayOfWeek = date.ToString("ddd", new System.Globalization.CultureInfo("vi-VN")).ToUpper(),
                                isToday = date.Date == DateTime.Today,
                                allocated = (double)allocated,
                                booked = (double)booked,
                                percentage = allocated > 0 ? Math.Round((booked / allocated) * 100, 0) : 0
                            });
                        }
                    }

                    // Chuyển đổi sang định dạng categories cuối cùng
                    foreach (var row in rowList)
                    {
                        categories.Add(new
                        {
                            hotelRoomId = row.hotelRoomId,
                            roomName = row.roomName,
                            totalCapacity = row.totalCapacity,
                            dailyData = row.dailyList
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
