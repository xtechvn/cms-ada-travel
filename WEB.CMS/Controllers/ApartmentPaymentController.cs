using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Utilities;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    public class ApartmentPaymentController : Controller
    {
        private readonly IApartmentOrderRepository _apartmentOrderRepository;
        private ManagementUser _ManagementUser;
        private readonly IWebHostEnvironment _WebHostEnvironment;


        public ApartmentPaymentController(IApartmentOrderRepository apartmentOrderRepository, IWebHostEnvironment hostEnvironment, ManagementUser managementUser)
        {
            _apartmentOrderRepository = apartmentOrderRepository;
            _WebHostEnvironment = hostEnvironment;
            _ManagementUser = managementUser;
        }
        /// <summary>
        /// Trang list phiếu chi cổ đông (tab riêng)
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Search danh sách phiếu chi (lọc theo tên cổ đông)
        /// </summary>
        [HttpPost]
        public IActionResult Search(string keyword, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<HotelShareHolderPaymentGridModel>();

            try
            {
                if (!string.IsNullOrEmpty(keyword))
                    keyword = keyword.Trim();

                long total;
                var data = _apartmentOrderRepository
                    .GetListPayment(keyword, currentPage, pageSize, out total);

                model.ListData = data;
                model.CurrentPage = currentPage;
                model.PageSize = pageSize;
                model.TotalRecord = total;
                model.TotalPage = (int)Math.Ceiling((double)total / pageSize);

                // Partial: _ApartmentPaymentGrid.cshtml
                return PartialView("_ApartmentPaymentGrid", model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - ApartmentPaymentController: " + ex);
                return PartialView("_ApartmentPaymentGrid", model);
            }
        }
        [HttpPost]
        public IActionResult SearchShareHolder(string keyword)
        {
            try
            {
                var data = _apartmentOrderRepository.SearchShareHolder(keyword);
                return Json(new { data });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SearchShareHolder error: " + ex);
                return Json(new { data = new List<object>() });
            }
        }


        /// <summary>
        /// Mở popup thêm phiếu chi cổ đông
        /// </summary>
        [HttpGet]
        public IActionResult Add()
        {
            var model = new HotelShareHolderPayment
            {
                PayDate = DateTime.Now
            };

            // View popup: _ApartmentPaymentAdd.cshtml
            return PartialView("_ApartmentPaymentAdd", model);
        }

        /// <summary>
        /// Lưu phiếu chi cổ đông
        /// </summary>
        [HttpPost]
        public IActionResult Add(HotelShareHolderPayment model)
        {
            try
            {
                if (model.ShareHolderId <= 0)
                    return Json(new { isSuccess = false, message = "Vui lòng chọn cổ đông" });

                if (model.Amount <= 0)
                    return Json(new { isSuccess = false, message = "Số tiền chi phải lớn hơn 0" });

                var currentUser = _ManagementUser.GetCurrentUser();
                model.CreatedBy = currentUser?.Id;

                var result = _apartmentOrderRepository.InsertPayment(model);

                if (result == 1)
                {
                    return Json(new
                    {
                        isSuccess = true,
                        message = "Thêm phiếu chi thành công"
                    });
                }
                else if (result == -1)
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Số tiền chi vượt quá số tiền còn phải trả"
                    });
                }
                else
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Thêm phiếu chi thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Add - ApartmentPaymentController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }


        /// <summary>
        /// Xóa phiếu chi (IsDeleted = 1)
        /// </summary>
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var result = _apartmentOrderRepository.DeletePayment(id);

                if (result > 0)
                {
                    return Json(new
                    {
                        isSuccess = true,
                        message = "Xóa phiếu chi thành công"
                    });
                }
                else
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Xóa phiếu chi thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Delete - ApartmentPaymentController: " + ex);
                return Json(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
    }
}
