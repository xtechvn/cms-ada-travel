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
                    model.HotelRoomFundDetails = await _hotelRoomFundRepository.GetListHotelRoomFundDetail(id);
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
