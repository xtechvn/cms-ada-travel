using Entities.ViewModels.FlightWarehouse;
using Entities.ViewModels.ElasticSearch;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Utilities;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Utilities.Contants;
using WEB.CMS.Customize;
using System.Collections.Generic;
using Entities.ViewModels.OrderManual;
using WEB.Adavigo.CMS.Service;
using System.Linq;
using Nest;
using Repositories.Repositories;
using Entities.Models;
using Entities.ViewModels;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class FlightWarehouseHoldTicketController : Controller
    {
        private readonly IFlightWarehouseHoldTicketRepository _flightWarehouseHoldTicketRepository;
        private readonly IFlightWarehouseRepository _flightWarehouseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IIdentifierServiceRepository _identifierServiceRepository;
        private readonly IAccountClientRepository _accountClientRepository;
        private readonly IFlyBookingDetailRepository _flyBookingDetailRepository;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ICustomerManagerRepository _customerManagerRepository;
        private readonly IndentiferService _indentiferService;

        public FlightWarehouseHoldTicketController(
            IFlightWarehouseHoldTicketRepository flightWarehouseHoldTicketRepository,
            IFlightWarehouseRepository flightWarehouseRepository,
            IUserRepository userRepository,
            IAllCodeRepository allCodeRepository,
            IOrderRepository orderRepository,
            IIdentifierServiceRepository identifierServiceRepository,
            IAccountClientRepository accountClientRepository,
            IFlyBookingDetailRepository flyBookingDetailRepository,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ICustomerManagerRepository customerManagerRepository,
            IContractPayRepository contractPayRepository)
        {
            _flightWarehouseHoldTicketRepository = flightWarehouseHoldTicketRepository;
            _flightWarehouseRepository = flightWarehouseRepository;
            _userRepository = userRepository;
            _allCodeRepository = allCodeRepository;
            _orderRepository = orderRepository;
            _identifierServiceRepository = identifierServiceRepository;
            _accountClientRepository = accountClientRepository;
            _flyBookingDetailRepository = flyBookingDetailRepository;
            _configuration = configuration;
            _customerManagerRepository = customerManagerRepository;
            _indentiferService = new IndentiferService(configuration, identifierServiceRepository, orderRepository, contractPayRepository);
        }

        public IActionResult Index()
        {
            var serviceType = _allCodeRepository.GetListByType("FWHTICKET_STATUS");
            ViewBag.Type = serviceType;
            return View();
        }

        public async Task<IActionResult> Search(FlightWarehouseHoldTicketSearchModel searchModel)
        {
            try
            {
                var data = await _flightWarehouseHoldTicketRepository.GetList(searchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - FlightWarehouseController: " + ex);
            }
            return PartialView(null);
        }

        public async Task<IActionResult> QuickOrderManual(long id, int bookingId)
        {
            try
            {
                var holdTicket = await _flightWarehouseHoldTicketRepository.GetById(id);
                var booking = await _flightWarehouseRepository.GetBookingById(bookingId);

                ViewBag.Branch = _allCodeRepository.GetListByType(AllCodeType.BRANCH_CODE);
                long _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                ViewBag.User_Create = new UserESViewModel();
                var User_Create = await _userRepository.GetById(_UserId);
                if (User_Create != null && User_Create.Id > 0)
                {
                    ViewBag.User_Create = new UserESViewModel()
                    {
                        email = User_Create.Email,
                        fullname = User_Create.FullName,
                        id = User_Create.Id,
                        phone = User_Create.Phone,
                        username = User_Create.UserName,
                        _id = User_Create.Id
                    };
                }

                ViewBag.User = new UserESViewModel();
                var user = await _userRepository.GetChiefofDepartmentByServiceType((int)ServiceType.PRODUCT_FLY_TICKET);
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

                ViewBag.HoldTicket = holdTicket;
                ViewBag.Booking = booking;
                var Segments = await _flightWarehouseRepository.GetSegmentsByBookingId(bookingId);
                ViewBag.Segments = Segments;
                var Prices = await _flightWarehouseRepository.GetPricesByBookingId(bookingId);
                ViewBag.Prices = Prices;

                return PartialView();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("QuickOrderManual - FlightWarehouseHoldTicketController: " + ex);
                return Content("Lỗi hệ thống");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQuickOrder(QuickOrderFlyModel data)
        {
            try
            {
                if (data == null || data.ClientId <= 0) return Json(new { status = 1, msg = "Dữ liệu không hợp lệ" });

                long _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                var holdTicket = await _flightWarehouseHoldTicketRepository.GetById(data.HoldTicketId);
                var booking = await _flightWarehouseRepository.GetBookingById(data.BookingId);
                var Segments = await _flightWarehouseRepository.GetSegmentsByBookingId(data.BookingId);
                var Prices = await _flightWarehouseRepository.GetPricesByBookingId(data.BookingId);
                var goSegment = Segments.FirstOrDefault(x => x.SegmentType == 0) ?? new FlightWarehouseSegmentModel();
                var backSegment = Segments.FirstOrDefault(x => x.SegmentType == 1);
                if (holdTicket == null || booking == null) return Json(new { status = 1, msg = "Không tìm thấy thông tin vé giữ" });

                if (holdTicket.Status == 1 || holdTicket.Status == 2)
                    return Json(new { status = 1, msg = "Vé này đã được chốt hoặc hủy." });


                int company_type = 0;
                try { company_type = Convert.ToInt32(_configuration["CompanyType"]); } catch { }

                // 1. Create Order
                var order = new Entities.Models.Order()
                {
                    OrderNo = await _indentiferService.buildOrderManual(company_type),
                    ClientId = data.ClientId,
                    SalerId = _UserId,
                    Label = data.Label,
                    BranchCode = (short)data.BranchCode,
                    Note = data.Note,
                    UserUpdateId = _UserId,
                    CreatedBy = _UserId,
                    CreateTime = DateTime.Now,
                    UpdateLast = DateTime.Now,
                    AccountClientId = _accountClientRepository.GetMainAccountClientByClientId(data.ClientId),
                    SystemType = (int)SystemType.Backend,
                    OrderStatus = (int)OrderStatus.CREATED_ORDER,
                    PaymentStatus = (int)PaymentStatus.UNPAID,
                    ExpriryDate = DateTime.Now.AddMonths(1),
                    Amount = data.Amount,
                    Profit = data.Profit,
                };

                var result = await _orderRepository.CreateOrder(order);
                var orderId = result.OrderId;
                if (orderId <= 0) return Json(new { status = 1, msg = "Không thể tạo đơn hàng" });

                // 2. Prepare Data for Fly Booking
                var summitData = new OrderManualFlyBookingServiceSummitModel
                {
                    order_id = orderId,
                    client_type = 0,
                    route = 1,
                    main_staff = data.OperatorId > 0 ? data.OperatorId : data.SalerId,
                    user_summit = (int)_UserId,
                    start_point = booking.DeparturePoint,
                    end_point = booking.ArrivalPoint,
                    start_date = goSegment.FlightDate ?? DateTime.Now,
                    end_date = goSegment.FlightDate ?? DateTime.Now,
                    service_code = await _indentiferService.buildServiceNo((int)ServicesType.FlyingTicket),
                    note = "",
                    others_amount = data.OthersAmount,
                    commission = data.Commission,
                    go = new OrderManualFlyBookingServiceSummitRoute
                    {
                        id = 0,
                        airline = goSegment.Airline,
                        fly_code = goSegment.FlightCode,
                        booking_code = goSegment.Pnrcode
                    },

                    extra_packages = new List<OrderManualFlyBookingServiceSummitRouteExtraPackage>(),
                    passenger = new List<OrderManualFlyBookingServiceSummitPassenger>()
                };
                if (Segments.Count > 1 && backSegment != null)
                {
                    summitData.back = new OrderManualFlyBookingServiceSummitRoute
                    {
                        id = 0,
                        airline = backSegment.Airline,
                        fly_code = backSegment.FlightCode,
                        booking_code = backSegment.Pnrcode
                    };
                }
                // Add packages (prices)
                if (holdTicket.AdultQuantity > 0)
                {
                    summitData.extra_packages.Add(new OrderManualFlyBookingServiceSummitRouteExtraPackage
                    {
                        id = 0,
                        package_code = "Người lớn",
                        package_id = "adt_amount",
                        quantity = data.AdultQuantity,
                        base_price = data.AdultPrice * data.AdultQuantity,
                        amount = data.AdultAmount * data.AdultQuantity,
                        profit = data.AdultAmount * data.AdultQuantity - data.AdultPrice * data.AdultQuantity,
                    });
                }
                if (holdTicket.ChildQuantity > 0)
                {
                    summitData.extra_packages.Add(new OrderManualFlyBookingServiceSummitRouteExtraPackage
                    {
                        id = 0,
                        package_code = "Trẻ em (2-14 tuổi)",
                        package_id = "chd_amount",
                        quantity = data.ChildQuantity,
                        base_price = data.ChildtPrice* data.ChildQuantity,
                        amount = data.ChildAmount* data.ChildQuantity,
                        profit = data.ChildAmount* data.ChildQuantity - data.ChildtPrice * data.ChildQuantity,
                    });
                }
                if (holdTicket.InfantQuantity > 0)
                {
                    summitData.extra_packages.Add(new OrderManualFlyBookingServiceSummitRouteExtraPackage
                    {
                        id = 0,
                        package_code = "Em bé (0-2 tuổi)",
                        package_id = "inf_amount",
                        quantity = data.InfantQuantity,
                        base_price = data.InfantPrice* data.InfantQuantity,
                        amount = holdTicket.InfantAmount* data.InfantQuantity,
                        profit = data.InfantAmount* data.InfantQuantity - data.InfantPrice * data.InfantQuantity
                    });
                }

                double totalOrderAmount = summitData.extra_packages.Sum(x => x.amount);
                int is_debt_able = await _orderRepository.IsClientAllowedToDebtNewService(totalOrderAmount, data.ClientId, orderId, (int)ServiceType.PRODUCT_FLY_TICKET);

                long flyBookingId = await _flyBookingDetailRepository.SummitFlyBookingServiceData(summitData, is_debt_able);
                if (flyBookingId <= 0) return Json(new { status = 1, msg = "Thêm mới dịch vụ vé máy bay thất bại" });

                // 3. Update Hold Ticket Status
               
                await _flightWarehouseHoldTicketRepository.UpdateFlightWarehouseHoldTicketStatus(data.HoldTicketId, (int)FlightWarehouseHoldTicketStatus.ISSUED);

                await _orderRepository.UpdateOrderDetail(orderId, _UserId);

                var list_order = _orderRepository.GetOrderByClientId(data.ClientId);
                if (list_order != null && (list_order.Count == 0 || list_order.Count == 1))
                {
                    await _customerManagerRepository.UpdateStatusClient((int)ClientStatus.CHOT, (int)data.ClientId);
                }

                return Json(new { status = 0, msg = "Tạo đơn hàng nhanh thành công. Mã đơn: " + order.OrderNo, order_id = orderId });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SubmitQuickOrder - FlightWarehouseHoldTicketController: " + ex.Message);
                return Json(new { status = 1, msg = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }

    
}
