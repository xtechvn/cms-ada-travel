using Caching.Elasticsearch;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.OrderManual;
using Entities.ViewModels.Request;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using PuppeteerSharp;
using Repositories.IRepositories;
using Repositories.Repositories;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.DeepSeekTravel.CMS.Service;
using WEB.CMS.Customize;
using WEB.CMS.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WEB.DeepSeekTravel.CMS.Controllers.Order
{
    [CustomAuthorize]
    public class RequestHotelBookingController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IRequestRepository _requestRepository;
        private readonly IHotelBookingRepositories _hotelBookingRepository;
        private UserESRepository _userESRepository;
        private HotelESRepository _hotelESRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly CommentService _commentService;
        private readonly ISubscriber _subscriber;
        private readonly IVoucherRepository _voucherRepository;
        private readonly ManagementUser _ManagementUser;
        private readonly IIdentifierServiceRepository _identifierServiceRepository;
        public RequestHotelBookingController(IConfiguration configuration, IUserRepository userRepository, IHotelBookingRepositories hotelBookingRepository,
            IRequestRepository requestRepository,IIdentifierServiceRepository identifierServiceRepository, IClientRepository clientRepository, IOrderRepository orderRepository, IHotelRepository hotelRepository,
            IHttpContextAccessor httpContextAccessor, IVoucherRepository voucherRepository, IContractPayRepository contractPayRepository, ManagementUser managementUser)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _hotelBookingRepository = hotelBookingRepository;
            _userESRepository = new UserESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _hotelESRepository = new HotelESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _requestRepository = requestRepository;
            _clientRepository = clientRepository;
            _orderRepository = orderRepository;
            _hotelRepository = hotelRepository;
            _commentService = new CommentService(configuration, httpContextAccessor);
            var connection = ConnectionMultiplexer.Connect(_configuration["Redis:Host"] + ":" + _configuration["Redis:Port"]);
            _subscriber = connection.GetSubscriber();
            _voucherRepository = voucherRepository;
            _identifierServiceRepository = identifierServiceRepository;
            _ManagementUser = managementUser;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Search(RequestSearchModel searchModel)
        {
            try
            {
                var data = await _requestRepository.GetPagingList(searchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - RequestHotelBookingController: " + ex);
            }
            return PartialView();

        }
        public async Task<IActionResult> Detail(long hotel_booking_id, long ClientId, long id)
        {
            try
            {
                var data_commit = new CommentComponentViewModel();
                ViewBag.data_commit = data_commit;
                ViewBag.PriceSalesVoucher = 0;
                if (id != 0)
                {
                    var comments = await _commentService.GetListCommentsAsync((int)id);
                    data_commit.RequestId = (int)id;
                    data_commit.Comments = comments ?? new List<CommentViewModel>();
                    ViewBag.data_commit = data_commit;
                    ViewBag.status = 0;
                    ViewBag.Unit = 1;
                    ViewBag.RequestId = id;
                    var detail = await _requestRepository.GetDetailRequest(id);
                    if (detail != null)
                    {
                        ViewBag.status = detail.Status;
                        ViewBag.saleName = detail.SalerName;
                        ViewBag.Email = detail.Email;
                        ViewBag.Discount = detail.Discount.ToString("N0");
                        if (detail.VoucherName != null && detail.VoucherName != "")
                        {
                            var voucher = await _voucherRepository.getDetailVoucher(detail.VoucherName);
                            ViewBag.PriceSalesVoucher = voucher.PriceSales;
                            switch (voucher.Unit)
                            {
                                case UnitVoucherType.PHAN_TRAM:
                                    //Tinh số tiền giảm theo %
                                    ViewBag.Unit = 0;
                                    break;
                                case UnitVoucherType.VIET_NAM_DONG:
                                    ViewBag.Unit = 1;
                                    break;
                            }
                        }

                    }

                }
                ViewBag.id = id;
                if (ClientId != 0)
                {
                    var UserCreateclient = await _clientRepository.GetClientDetailByClientId(ClientId);
                    if (UserCreateclient != null)
                    {
                        ViewBag.client = UserCreateclient;
                    }
                }

                long _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                ViewBag.User = new UserESViewModel();
                var user = await _userRepository.GetChiefofDepartmentByServiceType((int)ServiceType.BOOK_HOTEL_ROOM_VIN);
                if (user != null && user.Id > 0)
                {
                    ViewBag.User = new UserESViewModel()
                    {
                        email = user.Email,
                        fullname = user.FullName,
                        id = user.Id,
                        phone = user.Phone,
                        username = user.UserName,
                        _id = user.Id
                    };
                }
                ViewBag.IsOrderManual = true;
                ViewBag.AllowToEdit = true;
                ViewBag.HotelBooking = new HotelBooking();
                ViewBag.Hotel = new HotelESViewModel();
                var booking = await _hotelBookingRepository.GetHotelBookingByID(hotel_booking_id);
                if (booking != null && booking.Id > 0)
                {
                    ViewBag.IsOrderManual = false;
                    ViewBag.HotelBooking = booking;
                    var hotel = await _hotelESRepository.GetHotelByID(booking.PropertyId);
                    if (hotel == null) hotel = new HotelESViewModel();
                    ViewBag.Hotel = hotel;
                    var user_by_booking = await _userESRepository.GetUserByID(booking.SalerId.ToString());
                    if (user_by_booking != null && user_by_booking.id > 0)
                    {
                        ViewBag.User = user_by_booking;
                    }
                    ViewBag.IsOrderManual = false;
                    //if (order != null)
                    //{
                    //    ViewBag.IsOrderManual = true;
                    //}
                    bool is_allow_to_edit = false;
                    if (booking.SalerId != null && booking.SalerId == _UserId)
                    {
                        is_allow_to_edit = true;

                    }
                    ViewBag.AllowToEdit = is_allow_to_edit;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Detail - RequestHotelBookingController: " + ex);
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SummitHotelServiceData(OrderManualHotelSerivceSummitHotel data)
        {

            try
            {
                HotelESRepository _ESRepository = new HotelESRepository(_configuration["DataBaseConfig:Elastic:Host"], _configuration);
                HotelESViewModel hotel_detail = await _hotelESRepository.GetHotelByID(data.hotel.hotel_id);
                if (hotel_detail.checkintime <= DateTime.Now.AddDays(-30)) hotel_detail.checkintime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 0, 0);
                if (hotel_detail.checkouttime <= DateTime.Now.AddDays(-30)) hotel_detail.checkouttime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 0, 0);
                int user_id = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    user_id = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                //-- Check user & permission
                if (user_id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "You do not have permission to do this."
                    });
                }
                int? tenant_id = _ManagementUser.GetCurrentTenantId();

                //-- Check if order is manual Order:
                var exists_order = await _orderRepository.GetOrderByID(data.order_id);

                //-- Validate Data(server-side):
                if (data.hotel == null)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Dữ liệu khách sạn gửi lên không chính xác, vui lòng kiểm tra lại"
                    });
                }
                else if (data.rooms.Any(x => x.package == null || x.package.Count < 1))
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Mỗi phòng trong khách sạn phải có ít nhất 01 gói"
                    });
                }

                if (data.hotel.id <= 0 || data.hotel.service_code == null || data.hotel.service_code.Trim() == "")
                {
                    data.hotel.service_code = await _identifierServiceRepository.buildServiceNo((int)ServicesType.VINHotelRent, tenant_id);
                }

                #region Check Client Debt:
                long id = 0;
                double total_amount = 0;
                int service_status = (int)ServiceStatus.OnExcution;
                if (data.hotel.id <= 0)
                {
                    service_status = (int)ServiceStatus.New;
                    total_amount += data.rooms.Sum(x => x.package.Sum(x => x.amount));
                    total_amount += data.extra_package != null && data.extra_package.Count > 0 ? data.extra_package.Sum(x => x.amount) : 0;
                }
                else
                {
                    var exists_hotel = await _hotelBookingRepository.GetHotelBookingByID(data.hotel.id);
                    service_status = (int)exists_hotel.Status;
                    total_amount += data.rooms.Sum(x => x.package.Sum(x => x.amount));
                    total_amount += data.extra_package != null && data.extra_package.Count > 0 ? data.extra_package.Sum(x => x.amount) : 0;
                    total_amount -= exists_hotel.TotalAmount;
                }

                //int is_debt_able = await _orderRepository.IsClientAllowedToDebtNewService(total_amount, (long)exists_order.ClientId, exists_order.OrderId, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);

                #endregion
                //-- Update Booking
                id = await _hotelBookingRepository.UpdateHotelBooking(data, hotel_detail, user_id, 0);
                if (id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Thêm mới / Cập nhật dịch vụ vé máy bay thất bại, vui lòng liên hệ IT",
                        data = id
                    });

                }
                #region Update Request:
                var model = new Entities.Models.Request();
                model.RequestId = data.hotel.requestId;
                model.Discount = data.hotel.voucher;
                model.Price = data.hotel.amount > 0 ? data.hotel.amount - data.hotel.voucher : 0;
                model.Amount = data.hotel.amount;
                model.UpdatedBy = user_id;
                var update = await _requestRepository.UpdateRequest(model);
                #endregion
                #region Update Order Amount:
                //await _orderRepository.UpdateOrderDetail(data.order_id, user_id);
                //await _orderRepository.ReCheckandUpdateOrderPayment(data.order_id);
                #endregion

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Thêm mới / Cập nhật dịch vụ khách sạn thành công",
                    data = id
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SummitHotelServiceData - RequestHotelBookingController: " + ex.ToString());
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Thêm mới / Cập nhật dịch vụ khách sạn thất bại, vui lòng liên hệ IT"
            });

        }
        public async Task<IActionResult> UpdateStatus(int type, int id)
        {
            var status = (int)ResponseType.FAILED;
            var msg = "Không thành công";
            try
            {
                var model = new Entities.Models.Request();
                model.RequestId = id;
                model.Status = type;
                model.Discount = null;
                model.Amount = null;
                model.Price = null;
                model.Status = type;
                model.UpdatedBy = (int?)Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var update = await _requestRepository.UpdateRequest(model);
                if (update > 0)
                {
                    status = (int)ResponseType.SUCCESS;
                    msg = "xác nhận thành công";
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateStatus - RequestHotelBookingController: " + ex.ToString());
            }
            return Ok(new
            {
                status = status,
                msg = msg
            });
        }
        public async Task<IActionResult> CommentRequest(int id)
        {
            var data = new CommentComponentViewModel();
            try
            {
                ViewBag.id = id;

                if (id > 0)
                {
                    var comments = await _commentService.GetListCommentsAsync(id);

                    data.RequestId = id;
                    data.Comments = comments ?? new List<CommentViewModel>();
                }
                //@await Component.InvokeAsync("Comment", new { requestId = ViewBag.id })
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CommentRequest - RequestHotelBookingController: " + ex.ToString());
            }
            return PartialView(data);

        }
        [HttpGet]
        public async Task GetCommentsStream(int requestId)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");

            var dataQueue = new ConcurrentQueue<string>();

            _subscriber.Subscribe($"COMMENT_{requestId}", (channel, message) =>
            {
                dataQueue.Enqueue(message);
            });

            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                while (dataQueue.TryDequeue(out var message))
                {
                    var data = $"data: {message}\n\n";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
                    await Response.Body.WriteAsync(buffer, 0, buffer.Length);
                    await Response.Body.FlushAsync();
                }

                await Task.Delay(100); // Giảm tải CPU
            }
        }


        //// Load danh sách comment ban đầu
        [HttpPost]
        public async Task<IActionResult> LoadComments(int requestId)
        {
            try
            {
                var comments = await _commentService.GetListCommentsAsync(requestId);

                return Ok(new
                {
                    status = comments != null ? 1 : 0,
                    data = comments
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"LoadComments Error: {ex}");
                return Ok(new { status = 0, msg = "Failed to load comments." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromForm] int requestId, [FromForm] string content, [FromForm] List<IFormFile> attachFiles)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) && (attachFiles == null || !attachFiles.Any()))
                {
                    return BadRequest(new { status = 1, msg = "Vui lòng nhập nội dung hoặc đính kèm file." });
                }
                int userid = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);



                var type = 0;

                var createdBy = userid;
                var fileUrls = new List<string>();
                var fileNames = new List<string>();

                // Kiểm tra file và upload
                if (attachFiles != null && attachFiles.Any())
                {
                    long totalSize = attachFiles.Sum(f => f.Length);
                    if (totalSize > 25 * 1024 * 1024) // 25MB
                    {
                        return BadRequest(new { status = 1, msg = "Tổng dung lượng file vượt quá 25MB." });
                    }

                    foreach (var file in attachFiles)
                    {
                        var fileUrl = await UpLoadHelper.UploadFileOrImage(file, requestId, 35);
                        if (!string.IsNullOrEmpty(fileUrl))
                        {
                            fileUrls.Add(fileUrl);
                            fileNames.Add(file.FileName);
                        }
                    }
                }

                // Lưu comment vào database
                var newComment = await _commentService.AddCommentAsync(requestId, userid, type, content, fileUrls, fileNames, createdBy);

                if (newComment != null)
                {
                    return Ok(new { status = 0, msg = "Comment added successfully.", data = newComment });
                }

                return BadRequest(new { status = 1, msg = "Failed to add comment." });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"AddComment Error: {ex}");
                return StatusCode(500, new { status = 1, msg = "Internal server error." });
            }
        }
    }
}
