using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Repositories.Repositories;
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
    public class UserReserveHotelRoomFundController : Controller
    {
        private readonly IUserReserveHotelRoomFundRepository _userReserveHotelRoomFundRepository;
        private readonly IHotelRoomFundRepository _hotelRoomFundRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly ManagementUser _managementUser;
        private readonly ISupplierRepository _supplierRepository;


        public UserReserveHotelRoomFundController(IUserReserveHotelRoomFundRepository userReserveHotelRoomFundRepository,
            IHotelRoomFundRepository hotelRoomFundRepository,
            IHotelRepository hotelRepository,
            ManagementUser managementUser, ISupplierRepository supplierRepository)
        {
            _userReserveHotelRoomFundRepository = userReserveHotelRoomFundRepository;
            _hotelRoomFundRepository = hotelRoomFundRepository;
            _hotelRepository = hotelRepository;
            _managementUser = managementUser;
            _supplierRepository = supplierRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(UserReserveHotelRoomFundSearchModel searchModel)
        {
            var model = new GenericViewModel<UserReserveHotelRoomFundViewModel>();
            try
            {
                var datas = await _userReserveHotelRoomFundRepository.GetListUserReserveHotelRoomFund(searchModel);
                model.CurrentPage = searchModel.PageIndex;
                model.ListData = datas;
                model.PageSize = searchModel.PageSize;
                model.TotalRecord = datas != null && datas.Any() ? datas.First().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.PageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - UserReserveHotelRoomFundController: " + ex);
            }
            return PartialView(model);
        }

        public IActionResult StaffFundManagement(int hotelId = 0, int supplierId = 0)
        {
            ViewBag.HotelId = hotelId;
            ViewBag.SupplierId = supplierId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StaffFundManagementSearch(UserReserveHotelRoomFundSearchModel searchModel)
        {
            var model = new GenericViewModel<UserReserveHotelRoomFundViewModel>();
            try
            {
                if (!string.IsNullOrEmpty(searchModel.FromDateStr))
                {
                    searchModel.FromDate = DateUtil.StringToDate(searchModel.FromDateStr);
                }
                if (!string.IsNullOrEmpty(searchModel.ToDateStr))
                {
                    searchModel.ToDate = DateUtil.StringToDate(searchModel.ToDateStr);
                }

                var datas = await _userReserveHotelRoomFundRepository.GetListUserReserveHotelRoomFund(searchModel);
                
                // If filtering by date is not fully supported by DAL/SP, we can filter here after fetching.
                // However, let's assume DAL might be updated or we filter here if needed.
                if (datas != null && datas.Any())
                {
                    if (searchModel.FromDate.HasValue)
                        datas = datas.Where(x => x.StartDate >= searchModel.FromDate.Value).ToList();
                    if (searchModel.ToDate.HasValue)
                        datas = datas.Where(x => x.EndDate <= searchModel.ToDate.Value).ToList();
                }

                model.CurrentPage = searchModel.PageIndex;
                model.ListData = datas;
                model.PageSize = searchModel.PageSize;
                model.TotalRecord = datas != null && datas.Any() ? datas.First().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.PageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("StaffFundManagementSearch - UserReserveHotelRoomFundController: " + ex);
            }
            return PartialView("_StaffFundManagementSearch", model);
        }

        [HttpPost]
        public async Task<IActionResult> GetListStaffHolding(UserReserveHotelRoomFundSearchModel searchModel)
        {
            var model = new GenericViewModel<UserReserveHotelRoomFundViewModel>();
            try
            {
                searchModel.PageIndex = 1;
                searchModel.PageSize = 5; // Show only top 5 as per the snippet
                var datas = await _userReserveHotelRoomFundRepository.GetListUserReserveHotelRoomFund(searchModel);
                model.ListData = datas;
                model.TotalRecord = datas != null && datas.Any() ? datas.First().TotalRow : 0;
                
                ViewBag.HotelId = searchModel.HotelId;
                ViewBag.SupplierId = searchModel.SupplierId;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListStaffHolding - UserReserveHotelRoomFundController: " + ex);
            }
            return PartialView("_ListStaffHolding", model);
        }

        public async Task<IActionResult> AddOrUpdate(int id, int hotelId = 0, int supplierId = 0, int hotelRoomId = 0)
        {
            var model = new UserReserveHotelRoomFundViewModel();
            if (id > 0)
            {
                var data = await _userReserveHotelRoomFundRepository.GetById(id);
            }

            ViewBag.PreHotelId = hotelId;
            ViewBag.PreSupplierId = supplierId;
            ViewBag.PreHotelRoomId = hotelRoomId;

            if (hotelId > 0 && supplierId > 0)
            {
                var hotel = _hotelRepository.GetHotelById(hotelId);
                var supplier = _supplierRepository.GetById(supplierId);
                ViewBag.PreHotelName = hotel?.Name;
                ViewBag.PreSupplierName = supplier?.FullName;
            }

            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetHotelWithFund(string name)
        {
            try
            {
                var searchModel = new HotelRoomFundSearchMode { PageIndex = 1, PageSize = 100 };
                var funds = await _hotelRoomFundRepository.GetListHotelRoomFund(searchModel);

                if (!string.IsNullOrEmpty(name))
                {
                    funds = funds.Where(f => f.HotelName.ToLower().Contains(name.ToLower())).ToList();
                }

                // Nhóm theo HotelId để lấy danh sách khách sạn duy nhất
                var hotels = funds.GroupBy(f => f.HotelId).Select(g => new
                {
                    id = g.Key,
                    name = g.First().HotelName
                }).ToList();

                return Json(new { status = 0, data = hotels });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSupplierByHotel(int hotelId)
        {
            try
            {
                var searchModel = new HotelRoomFundSearchMode { HotelId = hotelId, PageIndex = 1, PageSize = 100 };
                var funds = await _hotelRoomFundRepository.GetListHotelRoomFund(searchModel);
                var result = funds.Select(f => new { id = f.SupplierId, name = f.SupplierName }).ToList();
                return Json(new { status = 0, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetRoomTypeList(int hotelId, int supplierId)
        {
            try
            {
                var fund = await _hotelRoomFundRepository.GetByHotelAndSupplier(hotelId, supplierId);
                if (fund == null) return Json(new { status = 0, data = new List<HotelRoomFundDetailModel>() });

                var rooms = await _hotelRoomFundRepository.GetListHotelRoomFundDetail(fund.Id);
                // Nhóm theo Room Id để lấy danh sách hạng phòng duy nhất
                var result = rooms.GroupBy(r => r.HotelRoomId).Select(g => new
                {
                    id = g.Key,
                    name = g.First().RoomName
                }).ToList();

                return Json(new { status = 0, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetRoomAvailability(int hotelId, int supplierId, int hotelRoomId, string startDate, string endDate)
        {
            try
            {
                DateTime startObj = DateTime.ParseExact(startDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                DateTime endObj = DateTime.ParseExact(endDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                var fund = await _hotelRoomFundRepository.GetByHotelAndSupplier(hotelId, supplierId);
                if (fund == null) return Json(new { status = 0, totalAvailable = 0 });

                int minAvail = int.MaxValue;
                bool hasData = false;

                // Lặp qua từng đêm để lấy số phòng trống nhỏ nhất trong khoảng
                for (var date = startObj; date < endObj; date = date.AddDays(1))
                {
                    var nightDetails = await _hotelRoomFundRepository.GetListHotelRoomFundDetail(fund.Id, date, date.AddDays(1));
                    var categoryNights = nightDetails?.Where(d => d.HotelRoomId == hotelRoomId).ToList();

                    if (categoryNights != null && categoryNights.Any())
                    {
                        // Tổng số phòng phân bổ của hạng phòng này trong đêm
                        int totalAllocated = (int)categoryNights[0].NumberOfRooms;
                        // Số phòng đã đặt (thường là giá trị aggregate của hạng phòng)
                        int totalBooked = categoryNights[0].TotalBookedRooms;

                        int avail = totalAllocated - totalBooked;
                        if (avail < minAvail) minAvail = avail;
                        hasData = true;
                    }
                    else
                    {
                        minAvail = 0;
                        hasData = true;
                        break;
                    }
                }

                if (!hasData || minAvail == int.MaxValue) minAvail = 0;

                return Json(new { status = 0, totalAvailable = minAvail });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, msg = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Save(int HotelId, int SupplierId, int HotelRoomId, int NumberOfRooms, string StartDate, string EndDate)
        {
            try
            {
                int userId = _managementUser.GetCurrentUser().Id;

                var model = new UserReserveHotelRoomFund
                {
                    UserId = userId,
                    HotelId = HotelId,
                    SupplierId = SupplierId,
                    HotelRoomId = HotelRoomId,
                    NumberOfRooms = NumberOfRooms,
                    CreatedDate = DateTime.Now,
                    Status = 0
                };

                if (!string.IsNullOrEmpty(StartDate))
                {
                    model.StartDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(EndDate))
                {
                    model.EndDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }

                var result = await _userReserveHotelRoomFundRepository.InsertUserReserveHotelRoomFund(model);
                if (result > 0)
                {
                    return Json(new { status = 0, msg = "Giữ phòng thành công" });
                }
                else
                {
                    return Json(new { status = 1, msg = "Giữ phòng thất bại" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Save - UserReserveHotelRoomFundController: " + ex);
                return Json(new { status = 1, msg = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
