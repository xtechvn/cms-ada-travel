using Entities.ViewModels;
using Entities.ViewModels.Mongo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.Service;

namespace WEB.Adavigo.CMS.Controllers
{
    [CustomAuthorize]
    public class QuotationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly ManagementUser _managementUser;
        private readonly QuotationMongoService _quotationMongoService;
        private readonly IAllCodeRepository _allCodeRepository;

        public QuotationController(
            IConfiguration configuration,
            IUserRepository userRepository,
            ManagementUser managementUser,
            IAllCodeRepository allCodeRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _managementUser = managementUser;
            _quotationMongoService = new QuotationMongoService(_configuration);
            _allCodeRepository = allCodeRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(
            string query, int status, long salerId, string fromDate, string toDate, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                DateTime? from = null;
                DateTime? to = null;

                if (!string.IsNullOrEmpty(fromDate))
                {
                    from = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(toDate))
                {
                    to = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                // Lấy danh sách phân trang
                var result = await _quotationMongoService.GetPagingList(query, status, salerId, from, to, pageIndex, pageSize);
                var listData = result.Item1;
                long totalRecord = result.Item2;

                // Lấy thống kê số lượng theo trạng thái
                var statusCounts = await _quotationMongoService.GetStatusCounts(query, salerId, from, to);

                var viewModel = new GenericViewModel<QuotationMongoModel>
                {
                    ListData = listData,
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalRecord = totalRecord,
                    TotalPage = (int)Math.Ceiling((double)totalRecord / pageSize)
                };

                ViewBag.StatusCounts = statusCounts;
                return PartialView("Search", viewModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("QuotationController - Search: " + ex);
                return PartialView("Search", new GenericViewModel<QuotationMongoModel>());
            }
        }

        public async Task<IActionResult> AddOrUpdate(string id)
        {
            var model = new QuotationMongoModel();
            try
            {
                // Load danh sách loại dịch vụ khác cho template
                var allService = _allCodeRepository.GetListByType(AllCodeType.SERVICE_TYPE_OTHER);
                var allServiceMain = _allCodeRepository.GetListByType(AllCodeType.SERVICE_TYPE_OTHER_MAIN).Select(x => x.CodeValue);
                if (allServiceMain != null && allServiceMain.Any())
                    allService = allService.Where(x => !allServiceMain.Contains(x.CodeValue)).ToList();
                ViewBag.OtherServiceTypes = allService;
                if (!string.IsNullOrEmpty(id))
                {
                    model = await _quotationMongoService.GetById(id);
                    if (model == null)
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    // Đặt mặc định ngày hết hạn là 7 ngày sau
                    model.CreatedDate = DateTime.Now;
                    model.ExpiredDate = DateTime.Now.AddDays(7);
                    model.Status = 0; // Nháp

                    var currentUser = _managementUser.GetCurrentUser();
                    if (currentUser != null)
                    {
                        model.CreatedBy = currentUser.Id;
                        model.SalerName = currentUser.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("QuotationController - AddOrUpdate View: " + ex);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(QuotationMongoModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { status = 1, msg = "Dữ liệu gửi lên rỗng hoặc sai định dạng" });
                }

                var currentUser = _managementUser.GetCurrentUser();
                if (currentUser == null)
                {
                    return Json(new { status = 1, msg = "Hết phiên làm việc. Vui lòng đăng nhập lại" });
                }

                if (string.IsNullOrEmpty(model._id))
                {
                    model.CreatedBy = currentUser.Id;
                    model.SalerName = currentUser.Name;
                }

                // Thực hiện tính toán lại tài chính trước khi lưu trữ
                CalculateFinancials(model);

                var savedId = await _quotationMongoService.SaveOrUpdate(model);
                if (!string.IsNullOrEmpty(savedId))
                {
                    return Json(new { status = 0, msg = "Lưu báo giá thành công", id = savedId });
                }
                else
                {
                    return Json(new { status = 1, msg = "Lưu thất bại lên MongoDB" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("QuotationController - Save: " + ex);
                return Json(new { status = 1, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> OtherServiceSuggestion(string txt_search)
        {
            try
            {
                var all_service = _allCodeRepository.GetListByType(AllCodeType.SERVICE_TYPE_OTHER);
                if (txt_search != null && txt_search!="")
                {
                    all_service = all_service.Where(s => s.Description.ToLower().Contains(txt_search.ToLower())).ToList();
                }
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = all_service
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("QuotationController - Delete: " + ex);
                return Json(new { status = 1, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { status = 1, msg = "Mã báo giá không hợp lệ" });
                }

                var success = await _quotationMongoService.Delete(id);
                if (success)
                {
                    return Json(new { status = 0, msg = "Xóa báo giá thành công" });
                }
                else
                {
                    return Json(new { status = 1, msg = "Xóa báo giá thất bại" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("QuotationController - Delete: " + ex);
                return Json(new { status = 1, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(string id, int status)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || status < 0 || status > 2)
                {
                    return Json(new { status = 1, msg = "Tham số trạng thái không hợp lệ" });
                }

                var model = await _quotationMongoService.GetById(id);
                if (model == null)
                {
                    return Json(new { status = 1, msg = "Không tìm thấy báo giá" });
                }

                model.Status = status;
                var savedId = await _quotationMongoService.SaveOrUpdate(model);

                if (!string.IsNullOrEmpty(savedId))
                {
                    return Json(new { status = 0, msg = "Cập nhật trạng thái thành công" });
                }
                else
                {
                    return Json(new { status = 1, msg = "Không thể cập nhật trạng thái" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("QuotationController - ChangeStatus: " + ex);
                return Json(new { status = 1, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        /// <summary>
        /// Logic phụ trợ tính toán tổng hợp tài chính từ các khối dịch vụ
        /// </summary>
        private void CalculateFinancials(QuotationMongoModel model)
        {
            double totalImport = 0;
            double totalExport = 0;

            // 1. Tích lũy Khách sạn
            if (model.Hotels != null)
            {
                foreach (var hotel in model.Hotels)
                {
                    double hotelImport = 0;
                    double hotelExport = 0;

                    if (hotel.Rooms != null)
                    {
                        foreach (var room in hotel.Rooms)
                        {
                            if (room.Package != null)
                            {
                                foreach (var pkg in room.Package)
                                {
                                    pkg.Amount = room.NumberOfRooms * pkg.SalePrice;
                                    pkg.Profit = pkg.Amount - (room.NumberOfRooms * pkg.OperatorPrice);
                                    
                                    hotelImport += room.NumberOfRooms * pkg.OperatorPrice;
                                    hotelExport += pkg.Amount;
                                }
                            }
                        }
                    }

                    if (hotel.ExtraPackages != null)
                    {
                        foreach (var ext in hotel.ExtraPackages)
                        {
                            ext.Amount = ext.NumberOfExtraPackages * ext.SalePrice;
                            ext.Profit = ext.Amount - (ext.NumberOfExtraPackages * ext.OperatorPrice);

                            hotelImport += ext.NumberOfExtraPackages * ext.OperatorPrice;
                            hotelExport += ext.Amount;
                        }
                    }

                    hotel.Amount = hotelExport;
                    totalImport += hotelImport;
                    totalExport += hotelExport;
                }
            }

            // 2. Tích lũy Vé máy bay
            if (model.Flights != null)
            {
                foreach (var flight in model.Flights)
                {
                    double flightImport = 0;
                    double flightExport = 0;

                    if (flight.ExtraPackages != null)
                    {
                        foreach (var ext in flight.ExtraPackages)
                        {
                            ext.Amount = ext.Quantity * ext.Amount; // Đảm bảo tính toán đúng
                            ext.Profit = ext.Amount - (ext.Quantity * ext.BasePrice);

                            flightImport += ext.Quantity * ext.BasePrice;
                            flightExport += ext.Amount;
                        }
                    }

                    flightImport += flight.Commission; // Cộng thêm chiết khấu/chi phí khác nếu có
                    flightExport += flight.AmountAdt + flight.AmountChd + flight.AmountInf + flight.OthersAmount;

                    flight.Profit = flightExport - flightImport;
                    totalImport += flightImport;
                    totalExport += flightExport;
                }
            }

            // 3. Tích lũy Tour du lịch
            if (model.Tours != null)
            {
                foreach (var tour in model.Tours)
                {
                    double tourImport = 0;
                    double tourExport = 0;

                    if (tour.ExtraPackages != null)
                    {
                        foreach (var ext in tour.ExtraPackages)
                        {
                            ext.Amount = ext.Quantity * ext.Amount;
                            ext.Profit = ext.Amount - (ext.Quantity * ext.BasePrice);

                            tourImport += ext.Quantity * ext.BasePrice;
                            tourExport += ext.Amount;
                        }
                    }

                    tourExport += tour.OtherAmount;
                    tour.Price = tourExport; // Tổng tiền bán
                    tour.Profit = tourExport - tourImport - tour.Commission - tour.FundCustomerCare;

                    totalImport += tourImport;
                    totalExport += tourExport;
                }
            }

            // 4. Dịch vụ khác
            if (model.Others != null)
            {
                foreach (var other in model.Others)
                {
                    double otherImport = 0;
                    double otherExport = 0;

                    if (other.Packages != null)
                    {
                        foreach (var pkg in other.Packages)
                        {
                            pkg.Amount = pkg.Quantity * pkg.SalePrice;
                            pkg.Profit = pkg.Amount - (pkg.Quantity * pkg.BasePrice);

                            otherImport += pkg.Quantity * pkg.BasePrice;
                            otherExport += pkg.Amount;
                        }
                    }

                    totalImport += otherImport;
                    totalExport += otherExport;
                }
            }

            // 5. VinWonder
            if (model.VinWonders != null)
            {
                foreach (var vin in model.VinWonders)
                {
                    double vinImport = 0;
                    double vinExport = 0;

                    if (vin.Packages != null)
                    {
                        foreach (var pkg in vin.Packages)
                        {
                            pkg.Amount = pkg.Quantity * pkg.Amount;
                            pkg.Profit = pkg.Amount - (pkg.Quantity * pkg.BasePrice);

                            vinImport += pkg.Quantity * pkg.BasePrice;
                            vinExport += pkg.Amount;
                        }
                    }

                    totalImport += vinImport;
                    totalExport += vinExport;
                }
            }

            // Đặt tổng các chỉ số cho Báo giá
            model.ServicesPriceImport = totalImport;
            model.ServicesPriceExport = totalExport;
            
            model.TotalPrice = totalExport;
            model.TotalProfit = totalExport - totalImport - model.OtherFees - model.CollaboratorComm - model.CustomerCareFund;
        }
    }
}
