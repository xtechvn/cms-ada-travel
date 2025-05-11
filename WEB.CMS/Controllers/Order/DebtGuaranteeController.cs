using Aspose.Cells;
using Caching.RedisWorker;
using Entities.ViewModels;
using Entities.ViewModels.Attachment;
using Entities.ViewModels.DebtGuarantee;
using Entities.ViewModels.HotelBookingCode;
using Entities.ViewModels.Mongo;
using Entities.ViewModels.Vinpearl;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nest;
using PdfSharp;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Linq;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;
using WEB.Adavigo.CMS.Service.ServiceInterface;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers.Order
{
    public class DebtGuaranteeController : Controller
    {
        private readonly IDebtGuaranteeRepository _debtGuaranteeRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private APIService apiService;
        private ManagementUser _ManagementUser;
        private readonly IEmailService _emailService;
        private readonly ITourRepository _tourRepository;
        private readonly IHotelBookingRepositories _hotelBookingRepositories;
        private readonly IFlyBookingDetailRepository _flyBookingDetailRepository;
        private IOtherBookingRepository _otherBookingRepository;
        private readonly IVinWonderBookingRepository _vinWonderBookingRepository;

        public DebtGuaranteeController(IDebtGuaranteeRepository debtGuaranteeRepository, IAllCodeRepository allCodeRepository, IUserRepository userRepository,
            IOrderRepository orderRepository, IConfiguration configuration, ManagementUser managementUser, IEmailService emailService, ITourRepository tourRepository, IHotelBookingRepositories hotelBookingRepositories, IFlyBookingDetailRepository flyBookingDetailRepository, IOtherBookingRepository otherBookingRepository, IVinWonderBookingRepository vinWonderBookingRepository)
        {
            _debtGuaranteeRepository = debtGuaranteeRepository;
            _allCodeRepository = allCodeRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            apiService = new APIService(configuration, userRepository);
            _ManagementUser = managementUser;
            _emailService = emailService;
            _tourRepository = tourRepository;
            _hotelBookingRepositories = hotelBookingRepositories;
            _flyBookingDetailRepository = flyBookingDetailRepository;
            _otherBookingRepository = otherBookingRepository;
            _vinWonderBookingRepository = vinWonderBookingRepository;
        }
        public IActionResult Index()
        {
            var STATUS = _allCodeRepository.GetListByType("DEBTSTATISTIC_STATUS");
            ViewBag.Status = STATUS;
            return View();
        }
        public async Task<IActionResult> GetList(SearchDebtGuarantee Searchmodel)
        {
            var model = new GenericViewModel<DebtGuaranteeViewModel>();
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                if (current_user != null)
                {
                    if (current_user.Role != "")
                    {
                        var list = current_user.Role.Split(',');
                        foreach (var item in list)
                        {
                            bool is_admin = false;
                            switch (Convert.ToInt32(item))
                            {
                                case (int)RoleType.SaleOnl:
                                case (int)RoleType.SaleKd:
                                case (int)RoleType.SaleTour:
                                case (int)RoleType.TPDHKS:
                                case (int)RoleType.TPDHVe:
                                case (int)RoleType.TPDHTour:
                                case (int)RoleType.TPKS:
                                case (int)RoleType.TPTour:
                                case (int)RoleType.TPVe:
                                case (int)RoleType.DHPQ:
                                case (int)RoleType.DHTour:
                                case (int)RoleType.DHVe:
                                case (int)RoleType.GDHN:
                                case (int)RoleType.GDHPQ:
                                    {
                                        if (Searchmodel.SalerPermission == null || Searchmodel.SalerPermission.Trim() == "")
                                        {
                                            Searchmodel.SalerPermission = current_user.UserUnderList;
                                        }
                                        else
                                        {
                                            Searchmodel.SalerPermission += "," + current_user.UserUnderList;

                                        }
                                    }
                                    break;
                                case (int)RoleType.Admin:
                                case (int)RoleType.KT:
                                case (int)RoleType.GD:
                                case (int)RoleType.PhoTPKeToan:
                                    {
                                        Searchmodel.SalerPermission = null;
                                        is_admin = true;
                                    }
                                    break;
                            }
                            if (is_admin) break;
                        }


                    }

                }
                model = await _debtGuaranteeRepository.GetListDebtGuarantee(Searchmodel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - DebtGuaranteeController: " + ex);
            }
            return PartialView(model);
        }
        public async Task<IActionResult> Detail(int id)
        {
            int _UserId = 0;
            ViewBag.tn = 0;
            ViewBag.tp = 0;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }
            var detail = await _debtGuaranteeRepository.GetDetailDebtGuarantee(id);
            var current_user = _ManagementUser.GetCurrentUser();
            if (current_user != null)
            {
                if (current_user.Role != "")
                {

                    var order = await _orderRepository.GetOrderByID(detail.OrderId);
                    var list = current_user.Role.Split(',');
                    foreach (var item in list)
                    {
                        bool is_admin = false;
                        switch (Convert.ToInt32(item))
                        {
                            case (int)RoleType.SaleOnl:
                            case (int)RoleType.SaleKd:
                            case (int)RoleType.SaleTour:
                            case (int)RoleType.TPDHKS:
                            case (int)RoleType.TPDHVe:
                            case (int)RoleType.TPDHTour:
                            case (int)RoleType.TPKS:
                            case (int)RoleType.TPTour:
                            case (int)RoleType.TPVe:
                            case (int)RoleType.DHPQ:
                            case (int)RoleType.DHTour:
                            case (int)RoleType.DHVe:
                            case (int)RoleType.GDHN:
                            case (int)RoleType.GDHPQ:
                                {
                                    var User = await _userRepository.GetDetailUser(_UserId);
                                    if (User != null)
                                    {
                                        if (current_user.UserUnderList.Contains(order.SalerId.ToString()) && User.Entity.UserPositionId == UserPositionType.TP && (detail.Status == (int)DebtGuaranteeStatus.CHO_TP_DUYET || detail.Status == (int)DebtGuaranteeStatus.CHO_TN_DUYET))
                                        {
                                            ViewBag.tp = 1;
                                        }
                                        if (current_user.UserUnderList.Contains(order.SalerId.ToString()) && User.Entity.UserPositionId == UserPositionType.TN && detail.Status == (int)DebtGuaranteeStatus.CHO_TN_DUYET )
                                        {
                                            ViewBag.tn = 1;
                                        }
                                    }

                                }
                                break;
                            case (int)RoleType.Admin:
                                {
                                    if (detail.Status == (int)DebtGuaranteeStatus.CHO_TP_DUYET || detail.Status == (int)DebtGuaranteeStatus.CHO_TN_DUYET)
                                        ViewBag.tp = 1;
                                }
                                break;
                        }
                        if (is_admin) break;
                    }
                }
            }

            //var Leaderid = await _userRepository.GetLeaderByUserId(Convert.ToInt64(order.SalerId));
            //var Tpsalerid = await _userRepository.GetManagerByUserId(Convert.ToInt64(order.SalerId));
            //if (Leaderid == _UserId)
            //{
            //    ViewBag.tn = 1;
            //}
            //if (Tpsalerid == _UserId)
            //{
            //    ViewBag.tp = 1;
            //}
            return PartialView(detail);
        }
        public async Task<IActionResult> UpdateStatus(int id, int status)
        {
            var sst_status = (int)ResponseType.SUCCESS;
            var smg = "Không thành công";
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var Update = await _debtGuaranteeRepository.UpdateDebtGuarantee(id, status, _UserId);
                if (Update > 0)
                {
                    smg = "Thành công";
                    if (status == (int)DebtGuaranteeStatus.TP_DUYET || status == (int)DebtGuaranteeStatus.TN_DUYET)
                    {



                        var orderStatus = _allCodeRepository.GetListByType("ORDER_STATUS");
                        var allCodes = orderStatus.Where(s => s.CodeValue == (int)OrderStatus.WAITING_FOR_OPERATOR).ToList();
                        var detail = await _debtGuaranteeRepository.GetDetailDebtGuarantee(id);
                        var user = await _userRepository.GetById(_UserId);
                        var order = await _orderRepository.GetOrderByID(detail.OrderId);
                        var modelLog = new LogActionModel();
                        modelLog.Type = (int)AttachmentType.OrderDetail;
                        modelLog.LogId = detail.OrderId;
                        modelLog.CreatedUserName = user.FullName;
                        modelLog.Log = allCodes[0].Description;
                        modelLog.Note = user.FullName + " công nợ đơn hàng";
                        var data = await _orderRepository.GetAllServiceByOrderId(detail.OrderId);
                        if (data != null)
                            foreach (var item in data)
                            {
                                item.Price += item.Profit;
                                if (item.Type.Equals("Tour"))
                                {
                                    item.tour = await _tourRepository.GetDetailTourByID(Convert.ToInt32(item.ServiceId));
                                }
                                if (item.Type.Equals("Khách sạn"))
                                {
                                    item.Hotel = await _hotelBookingRepositories.GetDetailHotelBookingByID(Convert.ToInt32(item.ServiceId));
                                }
                                if (item.Type.Equals("Vé máy bay"))
                                {
                                    item.Flight = await _flyBookingDetailRepository.GetDetailFlyBookingDetailById(Convert.ToInt32(item.ServiceId));
                                }
                                if (item.Type.Equals("Dịch vụ khác"))
                                {
                                    item.OtherBooking = await _otherBookingRepository.GetDetailOtherBookingById(Convert.ToInt32(item.ServiceId));
                                }
                                if (item.Type.Equals("Vinwonder"))
                                {
                                    item.VinWonderBooking = await _vinWonderBookingRepository.GetDetailVinWonderByBookingId(Convert.ToInt32(item.ServiceId));
                                }
                            }
                        if (data != null && data.Count > 1)
                        {
                            for (int o = 0; o < data.Count - 1; o++)
                            {

                                if (data[o].Flight != null && data[o + 1].Flight != null)
                                {
                                    if (data[o].Flight.GroupBookingId == data[o + 1].Flight.GroupBookingId && data[o].Flight.Leg != data[o + 1].Flight.Leg)
                                    {
                                        data[o].Flight.StartDistrict2 = data[o + 1].Flight.StartDistrict;
                                        data[o].Flight.EndDistrict2 = data[o + 1].Flight.EndDistrict;
                                        data[o].Flight.Leg2 = 3;
                                        data[o].Flight.BookingCode2 = data[o + 1].Flight.BookingCode;
                                        data[o].Amount = data[o].Flight.Amount + data[o + 1].Flight.Amount;
                                        data[o].EndDate = data[o + 1].EndDate;

                                        data.Remove(data[o + 1]);

                                    }
                                }

                            }
                        }
                        long status_order = Convert.ToInt32((int)OrderStatus.WAITING_FOR_OPERATOR);
                        var data2 = await _orderRepository.UpdateOrderFinishPayment(detail.OrderId, status_order);

                        string link = "/Order/" + detail.OrderId;
                        apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.CONG_NO_DON_HANG).ToString(), ((int)Utilities.Contants.ActionType.DA_DUYET_CONG_NO).ToString(), detail.OrderNo, link, current_user.Role, detail.OrderNo);
                        foreach (var item in data)
                        {
                            if (item.Type.Equals("Tour"))
                            {
                                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.DON_HANG).ToString(), ((int)Utilities.Contants.ActionType.DUYET_DICH_VU).ToString(), item.OrderNo, link, current_user.Role, item.tour.ServiceCode);
                            }
                            if (item.Type.Equals("Khách sạn"))
                            {
                                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.DON_HANG).ToString(), ((int)Utilities.Contants.ActionType.DUYET_DICH_VU).ToString(), item.OrderNo, link, current_user.Role, item.Hotel[0].ServiceCode);
                            }
                            if (item.Type.Equals("Vé máy bay"))
                            {
                                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.DON_HANG).ToString(), ((int)Utilities.Contants.ActionType.DUYET_DICH_VU).ToString(), item.OrderNo, link, current_user.Role, item.Flight.ServiceCode);
                            }
                            if (item.Type.Equals("Dịch vụ khác"))
                            {
                                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.DON_HANG).ToString(), ((int)Utilities.Contants.ActionType.DUYET_DICH_VU).ToString(), item.OrderNo, link, current_user.Role, item.OtherBooking[0].ServiceCode);
                            }
                            if (item.Type.Equals("Vinwonder"))
                            {
                                apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.DON_HANG).ToString(), ((int)Utilities.Contants.ActionType.DUYET_DICH_VU).ToString(), item.OrderNo, link, current_user.Role, item.VinWonderBooking[0].ServiceCode);
                            }
                        }
                        var modelEmail = new SendEmailViewModel();
                        modelEmail.Orderid = detail.OrderId;
                        modelEmail.ServiceType = (int)EmailType.SaleDH;
                        var attach_file = new List<AttachfileViewModel>();
                        bool resulstSendMail = await _emailService.SendEmail(modelEmail, attach_file);
                    }
                    else
                    {
                        var detail = await _debtGuaranteeRepository.GetDetailDebtGuarantee(id);
                        var user = await _userRepository.GetById(_UserId);
                        var order = await _orderRepository.GetOrderByID(detail.OrderId);
                        string link = "/Order/" + detail.OrderId;
                        apiService.SendMessage(_UserId.ToString(), ((int)ModuleType.CONG_NO_DON_HANG).ToString(), ((int)Utilities.Contants.ActionType.TU_CHOI_DON_CONG_NO).ToString(), detail.OrderNo, link, current_user.Role, detail.OrderNo);
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateStatus - DebtGuaranteeController: " + ex);
                sst_status = (int)ResponseType.FAILED;
            }

            return Ok(new
            {
                status = sst_status,
                smg = smg
            });
        }
    }
}
