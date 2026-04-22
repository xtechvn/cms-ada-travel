using APP_CHECKOUT.RabitMQ;
using Caching.Elasticsearch;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.ElasticSearch;
using Entities.ViewModels.OrderManual;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;
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
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IHotelBookingRepositories _hotelBookingRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IHotelBookingRoomRepository _hotelBookingRoomRepository;
        private readonly IHotelBookingRoomRatesRepository _hotelBookingRoomRatesRepository;
        private readonly IHotelBookingGuestRepository _hotelBookingGuestRepository;
        private readonly IIdentifierServiceRepository _identifierServiceRepository;
        private readonly IAccountClientRepository _accountClientRepository;
        private readonly HotelESRepository _hotelESRepository;
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient _workQueueClient;
        private readonly APIService _apiService;
        private readonly ICustomerManagerRepository _customerManagerRepository;


        public UserReserveHotelRoomFundController(IUserReserveHotelRoomFundRepository userReserveHotelRoomFundRepository,
            IHotelRoomFundRepository hotelRoomFundRepository,
            IHotelRepository hotelRepository,
            ManagementUser managementUser, ISupplierRepository supplierRepository,
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IHotelBookingRepositories hotelBookingRepository,
            IAllCodeRepository allCodeRepository,
            IHotelBookingRoomRepository hotelBookingRoomRepository,
            IHotelBookingRoomRatesRepository hotelBookingRoomRatesRepository,
            IHotelBookingGuestRepository hotelBookingGuestRepository,
            IIdentifierServiceRepository identifierServiceRepository,
            IAccountClientRepository accountClientRepository,
            ICustomerManagerRepository customerManagerRepository,
            IConfiguration configuration)
        {
            _userReserveHotelRoomFundRepository = userReserveHotelRoomFundRepository;
            _hotelRoomFundRepository = hotelRoomFundRepository;
            _hotelRepository = hotelRepository;
            _managementUser = managementUser;
            _supplierRepository = supplierRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _hotelBookingRepository = hotelBookingRepository;
            _allCodeRepository = allCodeRepository;
            _hotelBookingRoomRepository = hotelBookingRoomRepository;
            _hotelBookingRoomRatesRepository = hotelBookingRoomRatesRepository;
            _hotelBookingGuestRepository = hotelBookingGuestRepository;
            _identifierServiceRepository = identifierServiceRepository;
            _accountClientRepository = accountClientRepository;
            _configuration = configuration;
            _hotelESRepository = new HotelESRepository(_configuration["DataBaseConfig:Elastic:Host"], configuration);
            _workQueueClient = new WorkQueueClient(configuration);
            _apiService = new APIService(configuration, _userRepository); // Wait, I need IUserRepository
            _customerManagerRepository = customerManagerRepository;
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


        public async Task<IActionResult> QuickOrderManual(List<int> ids)
        {
            long _UserId = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserId = Convert.ToInt64(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            if (ids == null || !ids.Any())
            {
                return Content("Vui lòng chọn ít nhất một bản ghi.");
            }

            var listData = await _userReserveHotelRoomFundRepository.GetListByIds(ids);

            if (!listData.Any()) return Content("Không tìm thấy dữ liệu.");

            var first = listData.First();
            if (listData.Any(x => x.HotelId != first.HotelId || x.SupplierId != first.SupplierId))
            {
                return Content("Các phòng chọn phải cùng khách sạn và nhà cung cấp.");
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
            ViewBag.Branch = _allCodeRepository.GetListByType(AllCodeType.BRANCH_CODE);
            ViewBag.Hotel = _hotelRepository.GetHotelById(first.HotelId ?? 0);
            ViewBag.Supplier = _supplierRepository.GetById(first.SupplierId ?? 0);
            ViewBag.ListData = listData;
            ViewBag.TotalRooms = listData.Sum(x => x.NumberOfRooms);
            ViewBag.StartDate = listData.Min(x => x.StartDate);
            ViewBag.EndDate = listData.Max(x => x.EndDate);
            ViewBag.FundIds = string.Join(",", ids);

            return PartialView(listData);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitQuickOrder(QuickOrderRequestModel data, string fundIds)
        {
            try
            {
                if (data == null) return Json(new { status = 1, msg = "Dữ liệu không hợp lệ" });

                var currentUser = _managementUser.GetCurrentUser();
                var indentiferService = new IndentiferService(_configuration, _identifierServiceRepository, _orderRepository, null);

                int company_type = 0;
                try
                {
                    company_type = Convert.ToInt32(_configuration["CompanyType"]);
                }
                catch { }

                // 1. Create Order
                var order = new Entities.Models.Order()
                {
                    OrderNo = await indentiferService.buildOrderManual(company_type),
                    ClientId = data.ClientId,
                    SalerId = data.SalerId,
                    SalerGroupId = data.SubSalerIds != null ? string.Join(",", data.SubSalerIds) : string.Empty,
                    Label = data.Label,
                    BranchCode = (short)data.BranchCode,
                    Note = data.Note,
                    UserUpdateId = currentUser.Id,
                    CreatedBy = currentUser.Id,
                    CreateTime = DateTime.Now,
                    UpdateLast = DateTime.Now,
                    AccountClientId = _accountClientRepository.GetMainAccountClientByClientId(data.ClientId),
                    SystemType = (int)SystemType.Backend,
                    OrderStatus = (int)OrderStatus.CREATED_ORDER,
                    PaymentStatus = (int)PaymentStatus.UNPAID,
                    ExpriryDate = DateTime.Now.AddMonths(1),
                    Amount = 0
                };

                var result = await _orderRepository.CreateOrder(order);
                var orderId = result.OrderId;
                if (orderId <= 0) return Json(new { status = 1, msg = "Không thể tạo đơn hàng" });

                // 2. Prepare Data for Hotel Service
                var hotelES = await _hotelESRepository.GetHotelByID(data.HotelId.ToString());
                var fundIdList = new List<int>();
                if (!string.IsNullOrEmpty(fundIds))
                {
                    fundIdList = fundIds.Split(',').Select(int.Parse).ToList();
                }

                var fundData = await _userReserveHotelRoomFundRepository.GetListByIds(fundIdList);
                if (fundData != null && fundData.Any(x => x.Status == 1))
                {
                    return Json(new { status = 1, msg = "Một trong những phòng chọn đã được đặt trước đó. Vui lòng kiểm tra lại." });
                }

                var hotelDetail = new OrderManualHotelSerivceSummitHotelDetail
                {
                    id = 0, // New service
                    hotel_id = data.HotelId.ToString(),
                    hotel_name = hotelES?.name,
                    service_code = await indentiferService.buildServiceNo((int)ServicesType.VINHotelRent),
                    arrive_date = fundData.Any() ? fundData.Min(x => x.StartDate) : DateTime.Now,
                    departure_date = fundData.Any() ? fundData.Max(x => x.EndDate) : DateTime.Now,
                    number_of_rooms = fundData.Sum(x => x.NumberOfRooms),
                    number_of_adult = data.Adult,
                    number_of_child = data.Child,
                    number_of_infant = data.Infant,
                    other_amount = data.OtherAmount,
                    discount = data.Commission,
                    main_staff_id = data.OperatorId > 0 ? data.OperatorId : data.SalerId,
                    note = data.Note
                };

                var summitData = new OrderManualHotelSerivceSummitHotel
                {
                    order_id = orderId,
                    hotel = hotelDetail,
                    rooms = new List<OrderManualHotelSerivceSummitHotelRoom>(),
                    guest = new List<OrderManualHotelSerivceSummitGuest>()
                };

                int roomIndex = 1;
                foreach (var r in data.Rooms)
                {
                    var hbRoom = new OrderManualHotelSerivceSummitHotelRoom
                    {
                        id = 0,
                        room_no = roomIndex++,
                        room_type_id = r.RoomId.ToString(),
                        number_of_rooms = (short)r.NumberOfRooms,
                        room_type_name = r.RoomName,
                        package = new List<OrderManualHotelSerivceSummitHotelRoomRate>()
                    };

                    foreach(var p in r.Packages)
                    {
                        DateTime startDate = DateTime.ParseExact(p.StartDate, "dd/MM/yyyy", null);
                        DateTime endDate = DateTime.ParseExact(p.EndDate, "dd/MM/yyyy", null);
                        int nights = (int)(endDate - startDate).TotalDays;
                        if (nights == 0) nights = 1;

                        hbRoom.package.Add(new OrderManualHotelSerivceSummitHotelRoomRate
                        {
                            id = 0,
                            package_code = !string.IsNullOrEmpty(p.PackageName) ? p.PackageName : "Quỹ phòng",
                            from = startDate,
                            to = endDate,
                            operator_price = p.ImportPrice,
                            sale_price = p.ExportPrice,
                            amount = p.ExportPrice * r.NumberOfRooms * nights,
                            profit = (p.ExportPrice - p.ImportPrice) * r.NumberOfRooms * nights,
                            nights = (short)nights
                        });
                    }
                    summitData.rooms.Add(hbRoom);
                }

                if (data.Guests != null)
                {
                    foreach (var g in data.Guests)
                    {
                        DateTime? birthday = null;
                        if (!string.IsNullOrEmpty(g.Birthday)) {
                            try { birthday = DateTime.ParseExact(g.Birthday, "dd/MM/yyyy", null); } catch { }
                        }

                        summitData.guest.Add(new OrderManualHotelSerivceSummitGuest
                        {
                            id = 0,
                            name = g.Name,
                            note = g.Note,
                            type = (short)g.Type,
                            birthday = birthday,
                            room_no = g.RoomId > 0 ? g.RoomId : 1
                        });
                    }
                }

                double totalOrderAmount = summitData.rooms.Sum(x => x.package.Sum(p => p.amount));
                int is_debt_able = await _orderRepository.IsClientAllowedToDebtNewService(totalOrderAmount, data.ClientId, orderId, (int)ServiceType.BOOK_HOTEL_ROOM_VIN);

                long bookingId = await _hotelBookingRepository.UpdateHotelBooking(summitData, hotelES, currentUser.Id, is_debt_able);
                if (bookingId <= 0) return Json(new { status = 1, msg = "Thêm mới dịch vụ khách sạn thất bại" });

                // 3. Update Status of Reservations
                foreach (var fund in fundData)
                {
                    fund.Status = 1; // Mark as ordered
                    await _userReserveHotelRoomFundRepository.UpdateUserReserveHotelRoomFund(fund);
                }

                await _orderRepository.UpdateOrderDetail(orderId, currentUser.Id);

                _workQueueClient.SyncES(orderId, _configuration["DataBaseConfig:Elastic:SP:sp_GetOrder"], _configuration["DataBaseConfig:Elastic:Index:Order"], ProjectType.ADAVIGO_CMS, "SubmitQuickOrder UserReserveHotelRoomFundController");
                _workQueueClient.SyncES(bookingId, _configuration["DataBaseConfig:Elastic:SP:sp_GetHotelBooking"], _configuration["DataBaseConfig:Elastic:Index:HotelBooking"], ProjectType.ADAVIGO_CMS, "SubmitQuickOrder UserReserveHotelRoomFundController");

                string link = "/Order/OrderDetail/" + orderId;
                _apiService.SendMessage(currentUser.Id.ToString(), ((int)ModuleType.DON_HANG).ToString(), ((int)ActionType.TAO_MOI).ToString(), order.OrderNo, link, currentUser.Role);

                var list_order =  _orderRepository.GetOrderByClientId((long)order.ClientId);
                if (list_order != null && (list_order.Count == 0 || list_order.Count == 1))
                {
                    await _customerManagerRepository.UpdateStatusClient((int)ClientStatus.CHOT, (int)order.ClientId);
                }

                return Json(new { status = 0, msg = "Tạo đơn hàng nhanh thành công. Mã đơn: " + order.OrderNo, order_id = orderId });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SubmitQuickOrder - UserReserveHotelRoomFundController: " + ex);
                return Json(new { status = 1, msg = "Lỗi hệ thống: " + ex.Message });
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
        [HttpPost]
        public async Task<IActionResult> CancelReserve(int id)
        {
            try
            {
                var data = await _userReserveHotelRoomFundRepository.GetById(id);
                if (data == null) return Json(new { status = 1, msg = "Không tìm thấy thông tin" });

                var currentUser = _managementUser.GetCurrentUser();
                if (data.UserId != currentUser.Id)
                {
                    return Json(new { status = 1, msg = "Bạn không có quyền thực hiện thao tác này" });
                }

                if (data.Status == 1)
                {
                    return Json(new { status = 1, msg = "Phòng đã được đặt, không thể bỏ giữ" });
                }

                data.Status = 2; // Bỏ quỹ
                await _userReserveHotelRoomFundRepository.UpdateUserReserveHotelRoomFund(data);
                return Json(new { status = 0, msg = "Bỏ giữ phòng thành công" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CancelReserve - UserReserveHotelRoomFundController: " + ex);
                return Json(new { status = 1, msg = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}