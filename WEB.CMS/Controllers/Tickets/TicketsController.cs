using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.BankingAccount;
using Entities.ViewModels.Funding;
using Entities.ViewModels.Hotel;
using Entities.ViewModels.SupplierConfig;
using Entities.ViewModels.Tour;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using QRCoder;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Drawing.Imaging;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers.Tickets
{
    //[CustomAuthorize]
    public class TicketsController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly ICommonRepository _commonRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly WEB.CMS.Models.AppSettings config;
        private readonly IHotelBookingRoomRepository _hotelBookingRoomRepository;
        private readonly IOtherBookingRepository _otherBookingRepository;
        private readonly IFlyBookingDetailRepository _flyBookingDetailRepository;
        private readonly ITourPackagesOptionalRepository _tourPackagesOptionalRepository;
        private readonly IVinWonderBookingRepository _vinWonderBookingRepository;
        private readonly ITicketRepository _ticketRepo;
        private readonly IGroupProductRepository _GroupProductRepository;

        public TicketsController(IAllCodeRepository allCodeRepository, ISupplierRepository supplierRepository,
             IBrandRepository brandRepository, ICommonRepository commonRepository, IConfiguration _configuration, IWebHostEnvironment webHostEnvironment, IHotelBookingRoomRepository hotelBookingRoomRepository, IOtherBookingRepository otherBookingRepository, IFlyBookingDetailRepository flyBookingDetailRepository, ITourPackagesOptionalRepository tourPackagesOptionalRepository, IVinWonderBookingRepository vinWonderBookingRepository, ITicketRepository TicketRepository , IGroupProductRepository groupProductRepository)
        {
            _allCodeRepository = allCodeRepository;
            _supplierRepository = supplierRepository;
            _brandRepository = brandRepository;
            _commonRepository = commonRepository;
            config = ReadFile.LoadConfig();
            configuration = _configuration;
            _WebHostEnvironment = webHostEnvironment;
            _hotelBookingRoomRepository = hotelBookingRoomRepository;
            _otherBookingRepository = otherBookingRepository;
            _flyBookingDetailRepository = flyBookingDetailRepository;
            _tourPackagesOptionalRepository = tourPackagesOptionalRepository;
            _vinWonderBookingRepository = vinWonderBookingRepository;
            _ticketRepo = TicketRepository;
            _GroupProductRepository = groupProductRepository;
        }

        public IActionResult Index()
        {
            ViewBag.ServiceTypes = _allCodeRepository.GetListByType("SERVICE_TYPE");
            //ViewBag.Provinces = await _commonRepository.GetProvinceList();
            //ViewBag.Brands = await _brandRepository.GetAll();
            return View();
        }

        
        [HttpPost]
        public IActionResult Search(SupplierSearchModel searchModel)
        {
            var model = new GenericViewModel<SupplierTicketViewModel>();
            try
            {
                var listSuppliers = _supplierRepository.GetSuppliersForTickets(searchModel);
                model.CurrentPage = searchModel.currentPage;
                model.ListData = listSuppliers;
                model.PageSize = searchModel.pageSize;
                model.TotalRecord = listSuppliers != null && listSuppliers.Any() ? listSuppliers.FirstOrDefault().TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - SupplierController: " + ex);
            }
            return PartialView(model);
        }
        [HttpGet]
        public IActionResult GetDetail(int id)
        {
            try
            {
                var data = _ticketRepo.GetDetail(id);
                if (data != null)
                {
                    return Json(new
                    {
                        isSuccess = true,
                        data
                    });
                }
                else
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Không tìm thấy vé có ID: " + id
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - TicketController: " + ex);
                return Json(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        public IActionResult AddOrUpdate(int id, int supplierId = 0)
        {
            var model = new TicketListItemViewModel();
            if (id > 0)
            {
                model = _ticketRepo.GetDetail(id);
            }
            else
            {
                // tạo mới: lấy supplierId từ query
                model.SupplierId = supplierId;
            }
            var serviceTypes = _allCodeRepository.GetListByType("TICKET_STATUS")
                                     .Where(x => x.CodeValue != 2)
                                     .ToList();
            ViewBag.ServiceTypes = serviceTypes;
            ViewBag.TargetAudiences = _allCodeRepository.GetListByType("TARGETAUDIENCE_STATUS"); // ✅ load dropdown đối tượng



            return View(model); // trả PartialView modal
        }


        [HttpPost]
            public IActionResult Create(TicketListItemViewModel model)
            {
                try
                {
                    var result = _ticketRepo.InsertTicket(model);
                    if (result != null)
                    {
                        return new JsonResult(new { isSuccess = true, message = "Thêm vé thành công" });
                    }
                    return new JsonResult(new { isSuccess = false, message = "Thêm vé thất bại" });
                }
                catch (Exception ex)
                {
                    LogHelper.InsertLogTelegram("Create - TicketController: " + ex);
                    return new JsonResult(new { isSuccess = false, message = ex.Message });
                }
            }
        // ========== Sinh ảnh QR (hiển thị trong vé) ==========
       
        [HttpGet]
        public IActionResult GenerateQR(string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Không có mã QR hợp lệ!");

            // 🔥 Tạo URL động dẫn tới action Scan
            string qrUrl = Url.Action("Scan", "Tickets", new { code = code }, Request.Scheme);

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCode(qrData))
                using (var bitmap = qrCode.GetGraphic(20))
                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    return File(stream.ToArray(), "image/png");
                }
            }
        }
        [AllowAnonymous] // Cho phép quét QR mà không cần đăng nhập  
        [HttpGet]
       
        public IActionResult Scan(string code)
        {
            if (string.IsNullOrEmpty(code))
                return Content("<h3 style='color:red;text-align:center;'>❌ Mã vé không hợp lệ!</h3>", "text/html");

            var ticket = _ticketRepo.GetByCode(code);
            if (ticket == null)
                return Content("<h3 style='color:red;text-align:center;'>❌ Vé không tồn tại!</h3>", "text/html");

            if (ticket.Status != 1)
            {
                return Content("<h3 style='color:orange;text-align:center;'>⚠️ Vé đã được sử dụng hoặc hết hạn!</h3>", "text/html");
            }

            // ✅ Cập nhật Status = 2 (đã sử dụng)
            _ticketRepo.UpdateStatusByCode(code, 2);

            // ✅ Hiển thị giao diện xác nhận
            return Content($@"
        <div style='text-align:center;padding:40px;font-family:sans-serif;'>
            <img src='https://vinwonders.com/wp-content/uploads/2020/09/VinKE-logo.png' style='height:60px;margin-bottom:10px;' />
            <h2 style='color:green;'>✅ Vé hợp lệ!</h2>
            <p>Mã vé: <b>{ticket.TicketCode}</b></p>
            <p>Đã được xác nhận sử dụng lúc {DateTime.Now:HH:mm:ss dd/MM/yyyy}</p>
        </div>", "text/html");
        }
        [HttpGet]
        public IActionResult GetTicketStatus()
        {
            try
            {
                var list = _allCodeRepository.GetListByType("TICKET_STATUS");
                if (list == null || !list.Any())
                    return Json(new { success = false });

                var result = list.Select(x => new
                {
                    id = x.CodeValue,
                    name = x.Description
                }).ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTicketStatus - TicketController: " + ex);
                return Json(new { success = false, message = ex.Message });
            }
        }



        // ========== Hiển thị vé (popup “Xem vé”) ==========
        [HttpGet]
        public IActionResult ShowTicket(int id)
        {
            var ticket = _ticketRepo.GetDetail(id);
            if (ticket == null)
                return Content("Không tìm thấy vé");

            if (ticket.Status != 1)
                return Content("<div class='p-4 text-center text-danger fw-bold'>Vé đã được sử dụng hoặc không hợp lệ!</div>", "text/html");

            return PartialView("_TicketPopup", ticket);
        }

        // ========== Cập nhật sau khi quét QR ==========
        [AllowAnonymous]
        [HttpPost]
        public IActionResult MarkAsUsed(string code)
        {
            try
            {
                if (string.IsNullOrEmpty(code))
                    return Json(new { isSuccess = false, message = "Mã vé không hợp lệ!" });

                var success = _ticketRepo.UpdateStatusByCode(code, 2);
                if (!success)
                    return Json(new { isSuccess = false, message = "Vé đã được sử dụng hoặc hết hạn!" });

                return Json(new { isSuccess = true, message = "Đã xác nhận sử dụng vé!" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("MarkAsUsed - TicketsController: " + ex);
                return Json(new { isSuccess = false, message = "Lỗi hệ thống!" });
            }
        }


        [HttpPost]
            public IActionResult Update(TicketListItemViewModel model)
            {
                try
                {
                    var result = _ticketRepo.UpdateTicket(model);
                    if (result)
                    {
                        return new JsonResult(new { isSuccess = true, message = "Cập nhật vé thành công" });
                    }
                    return new JsonResult(new { isSuccess = false, message = "Cập nhật vé thất bại" });
                }
                catch (Exception ex)
                {
                    LogHelper.InsertLogTelegram("Update - TicketController: " + ex);
                    return new JsonResult(new { isSuccess = false, message = ex.Message });
                }
            }

        [HttpGet]
        public async Task<IActionResult> GetCategories(int parentId)
        {
            var list = await _GroupProductRepository.GetListByParentId(parentId);
            if (list == null || !list.Any())
                return Json(new { success = false, data = new List<object>() });

            var result = list.Select(x => new
            {
                id = x.Id,
                name = x.Name
            }).ToList();

            return Json(new { success = true, data = result });
        }


        [HttpGet]
        public IActionResult Detail(
    int id, int pageIndex = 1, int pageSize = 10,
    string search = null, int? status = null,
    int? playZoneId = null, int? categoryId = null, int? ticketTypeId = null,
    DateTime? expiredDate = null)
        {
            ViewBag.ServiceTypes = _allCodeRepository.GetListByType("TICKET_STATUS");

            var filter = new TicketListFilter
            {
                SupplierId = id,
                PageIndex = pageIndex < 1 ? 1 : pageIndex,
                PageSize = pageSize <= 0 ? 10 : pageSize,
                Search = search,
                Status = status,
                PlayZoneId = playZoneId,
                CategoryId = categoryId,
                TicketTypeId = ticketTypeId,
                ExpiredDate = expiredDate
            };

            var paged = _ticketRepo.GetListBySupplier(filter);
            ViewBag.Filter = filter;

            // Nếu load bằng AJAX để chỉ thay bảng
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_TicketTable", paged);

            return View(paged);
        }
        // ====== API FILTER DÙNG CHO NÚT "LỌC" (gọi Partial) ======
        [HttpGet]
        public IActionResult Filter(
            int supplierId, int pageIndex = 1, int pageSize = 10,
            string search = null, int? status = null,
            int? playZoneId = null, int? categoryId = null, int? ticketTypeId = null,
            DateTime? expiredDate = null)
        {
            try
            {
                var filter = new TicketListFilter
                {
                    SupplierId = supplierId,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    Search = search,
                    Status = status,
                    PlayZoneId = playZoneId,
                    CategoryId = categoryId,
                    TicketTypeId = ticketTypeId,
                    ExpiredDate = expiredDate
                };

                var paged = _ticketRepo.GetListBySupplier(filter);
                ViewBag.Filter = filter;

                return PartialView("_TicketTable", paged);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TicketsController.Filter: " + ex);
                return Content("Lỗi tải dữ liệu");
            }
        }
    }
}
